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
}
