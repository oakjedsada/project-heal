using Microsoft.EntityFrameworkCore;
using MindCheck.Application.Abstractions;
using MindCheck.Domain.Entities;
using MindCheck.Domain.ValueObjects;

namespace MindCheck.Infrastructure.Repositories;

public sealed class InstrumentRepository : IInstrumentRepository
{
    private readonly MindCheckDbContext _db;

    public InstrumentRepository(MindCheckDbContext db)
    {
        _db = db;
    }

    public Task<Instrument?> GetByIdAsync(InstrumentId id, CancellationToken cancellationToken) =>
        _db.Instruments
            .Include(i => i.Questions)
            .ThenInclude(q => q.Choices)
            .AsSplitQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

    public async Task<IReadOnlyList<ScoringRule>> GetScoringRulesAsync(InstrumentId id, CancellationToken cancellationToken) =>
        await _db.ScoringRules.AsNoTracking().Where(r => r.InstrumentId == id).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<RiskRule>> GetRiskRulesAsync(InstrumentId id, CancellationToken cancellationToken) =>
        await _db.RiskRules.AsNoTracking().Where(r => r.InstrumentId == id).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Instrument>> GetAllAsync(CancellationToken cancellationToken) =>
        await _db.Instruments.AsNoTracking().ToListAsync(cancellationToken);

    public Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken) =>
        _db.Instruments.AsNoTracking().AnyAsync(i => i.Code == code, cancellationToken);

    public async Task<Instrument> CreateFullInstrumentAsync(
        Instrument instrumentWithQuestions,
        IReadOnlyList<ScoringRuleSpec> scoringRuleSpecs,
        IReadOnlyList<RiskRuleSpec> riskRuleSpecs,
        CancellationToken cancellationToken)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

        // Phase 1: save the instrument + its Questions/Choices graph so the
        // database assigns real ids — scoring/risk rules need those ids and
        // can't be built until this round-trip completes.
        _db.Instruments.Add(instrumentWithQuestions);
        await _db.SaveChangesAsync(cancellationToken);

        var questionIdByOrderNo = instrumentWithQuestions.Questions.ToDictionary(q => q.OrderNo, q => q.Id);

        var scoringRules = scoringRuleSpecs
            .Select(spec => new ScoringRule(
                0,
                instrumentWithQuestions.Id,
                new ScoreRange(spec.Min, spec.Max),
                spec.Level,
                spec.Interpretation,
                spec.Advice))
            .ToList();

        var riskRules = riskRuleSpecs
            .Select(spec => new RiskRule(
                0,
                instrumentWithQuestions.Id,
                questionIdByOrderNo[spec.QuestionOrderNo],
                new RiskCondition(spec.Operator, spec.Threshold),
                spec.Action))
            .ToList();

        _db.ScoringRules.AddRange(scoringRules);
        _db.RiskRules.AddRange(riskRules);
        await _db.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return instrumentWithQuestions;
    }

    public async Task<Instrument> UpdateFullInstrumentAsync(
        InstrumentId instrumentId,
        string code,
        string name,
        string version,
        string source,
        bool isActive,
        IReadOnlyList<QuestionUpdateSpec> questionSpecs,
        IReadOnlyList<QuestionId> questionIdsToDelete,
        IReadOnlyList<ChoiceId> choiceIdsToDelete,
        IReadOnlyList<ScoringRuleUpdateSpec> scoringRuleSpecs,
        IReadOnlyList<int> scoringRuleIdsToDelete,
        CancellationToken cancellationToken)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

        // Tracked (no AsNoTracking) so the domain mutator calls below are
        // picked up by EF's change tracker: removing a Question/Choice from
        // its parent's collection becomes a DELETE, a new one added becomes
        // an INSERT, an edited scalar becomes an UPDATE — all from one
        // SaveChanges call, same mechanism CreateFullInstrumentAsync relies
        // on for the initial insert.
        var instrument = await _db.Instruments
            .Include(i => i.Questions)
            .ThenInclude(q => q.Choices)
            .AsSplitQuery()
            .FirstAsync(i => i.Id == instrumentId, cancellationToken);

        instrument.ChangeDetails(code, name, version, source, isActive);

        foreach (var questionId in questionIdsToDelete)
        {
            instrument.RemoveQuestion(questionId);
        }

        foreach (var choiceId in choiceIdsToDelete)
        {
            var owningQuestion = instrument.Questions.First(q => q.Choices.Any(c => c.Id == choiceId));
            owningQuestion.RemoveChoice(choiceId);
        }

        foreach (var questionSpec in questionSpecs)
        {
            if (questionSpec.QuestionId is { } existingQuestionId)
            {
                var question = instrument.Questions.First(q => q.Id == new QuestionId(existingQuestionId));
                question.ChangeDetails(questionSpec.Text, questionSpec.OrderNo);

                foreach (var choiceSpec in questionSpec.Choices)
                {
                    if (choiceSpec.ChoiceId is { } existingChoiceId)
                    {
                        var choice = question.Choices.First(c => c.Id == new ChoiceId(existingChoiceId));
                        choice.ChangeDetails(choiceSpec.Label, choiceSpec.Score, choiceSpec.OrderNo);
                    }
                    else
                    {
                        question.AddChoice(new Choice(
                            new ChoiceId(0), question.Id, choiceSpec.Label, choiceSpec.Score, choiceSpec.OrderNo));
                    }
                }
            }
            else
            {
                var newChoices = questionSpec.Choices
                    .Select(c => new Choice(new ChoiceId(0), new QuestionId(0), c.Label, c.Score, c.OrderNo))
                    .ToList();
                instrument.AddQuestion(new Question(
                    new QuestionId(0), instrument.Id, questionSpec.OrderNo, questionSpec.Text,
                    QuestionType.SingleChoice, newChoices));
            }
        }

        await _db.SaveChangesAsync(cancellationToken);

        // ScoringRules aren't part of Instrument's own navigation graph
        // (queried separately elsewhere too — see GetScoringRulesAsync), so
        // they're handled as their own tracked set, still inside the same
        // transaction.
        if (scoringRuleIdsToDelete.Count > 0)
        {
            await _db.ScoringRules
                .Where(r => scoringRuleIdsToDelete.Contains(r.Id))
                .ExecuteDeleteAsync(cancellationToken);
        }

        var trackedScoringRules = await _db.ScoringRules
            .Where(r => r.InstrumentId == instrumentId)
            .ToListAsync(cancellationToken);

        foreach (var ruleSpec in scoringRuleSpecs)
        {
            if (ruleSpec.ScoringRuleId is { } existingRuleId)
            {
                var rule = trackedScoringRules.First(r => r.Id == existingRuleId);
                rule.ChangeDetails(
                    new ScoreRange(ruleSpec.Min, ruleSpec.Max), ruleSpec.Level, ruleSpec.Interpretation, ruleSpec.Advice);
            }
            else
            {
                _db.ScoringRules.Add(new ScoringRule(
                    0, instrumentId, new ScoreRange(ruleSpec.Min, ruleSpec.Max),
                    ruleSpec.Level, ruleSpec.Interpretation, ruleSpec.Advice));
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return instrument;
    }
}
