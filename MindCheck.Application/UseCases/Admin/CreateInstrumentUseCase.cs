using MindCheck.Application.Abstractions;
using MindCheck.Application.Dtos.Admin;
using MindCheck.Application.Exceptions;
using MindCheck.Domain.Entities;
using MindCheck.Domain.ValueObjects;

namespace MindCheck.Application.UseCases.Admin;

public sealed class CreateInstrumentUseCase
{
    private readonly IInstrumentRepository _instrumentRepository;

    public CreateInstrumentUseCase(IInstrumentRepository instrumentRepository)
    {
        _instrumentRepository = instrumentRepository;
    }

    public async Task<CreateInstrumentResponse> ExecuteAsync(
        CreateInstrumentRequest request,
        CancellationToken cancellationToken)
    {
        if (await _instrumentRepository.CodeExistsAsync(request.Code, cancellationToken))
        {
            throw new DuplicateInstrumentCodeException(request.Code);
        }

        if (request.Questions.Count == 0)
        {
            throw new InvalidAdminRequestException("An instrument must have at least one question.");
        }

        if (request.ScoringRules.Count == 0)
        {
            throw new InvalidAdminRequestException("An instrument must have at least one scoring rule.");
        }

        var orderNosSeen = new HashSet<int>();
        var questions = new List<Question>();
        foreach (var q in request.Questions)
        {
            if (!orderNosSeen.Add(q.OrderNo))
            {
                throw new InvalidAdminRequestException($"Duplicate question order number {q.OrderNo}.");
            }

            if (q.Choices.Count == 0)
            {
                throw new InvalidAdminRequestException($"Question at order {q.OrderNo} must have at least one choice.");
            }

            var choices = q.Choices
                .Select(c => new Choice(new ChoiceId(0), new QuestionId(0), c.Label, c.Score, c.OrderNo))
                .ToList();

            questions.Add(new Question(
                new QuestionId(0),
                new InstrumentId(0),
                q.OrderNo,
                q.Text,
                QuestionType.SingleChoice,
                choices));
        }

        var scoringRuleSpecs = request.ScoringRules
            .Select(r => new ScoringRuleSpec(r.Min, r.Max, r.Level, r.Interpretation, r.Advice))
            .ToList();

        var riskRuleSpecs = new List<RiskRuleSpec>();
        foreach (var r in request.RiskRules)
        {
            if (!orderNosSeen.Contains(r.QuestionOrderNo))
            {
                throw new InvalidAdminRequestException(
                    $"Risk rule references question order {r.QuestionOrderNo}, which was not submitted.");
            }

            if (!Enum.TryParse<RiskOperator>(r.Operator, ignoreCase: true, out var op))
            {
                throw new InvalidAdminRequestException($"Unknown risk operator '{r.Operator}'.");
            }

            riskRuleSpecs.Add(new RiskRuleSpec(r.QuestionOrderNo, op, r.Threshold, r.Action));
        }

        var instrument = new Instrument(
            new InstrumentId(0),
            request.Code,
            request.Name,
            request.Version,
            request.Source,
            request.IsActive,
            questions);

        var saved = await _instrumentRepository.CreateFullInstrumentAsync(
            instrument,
            scoringRuleSpecs,
            riskRuleSpecs,
            cancellationToken);

        var questionDtos = saved.Questions
            .OrderBy(q => q.OrderNo)
            .Select(q => new CreatedQuestionDto(
                q.Id.Value,
                q.Text,
                q.OrderNo,
                q.Choices
                    .OrderBy(c => c.OrderNo)
                    .Select(c => new CreatedChoiceDto(c.Id.Value, c.Label, c.Score, c.OrderNo))
                    .ToList()))
            .ToList();

        return new CreateInstrumentResponse(saved.Id.Value, saved.Code, questionDtos);
    }
}
