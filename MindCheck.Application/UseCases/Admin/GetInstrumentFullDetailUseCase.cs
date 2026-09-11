using MindCheck.Application.Abstractions;
using MindCheck.Application.Dtos.Admin;
using MindCheck.Application.Exceptions;
using MindCheck.Domain.ValueObjects;

namespace MindCheck.Application.UseCases.Admin;

public sealed class GetInstrumentFullDetailUseCase
{
    private readonly IInstrumentRepository _instrumentRepository;

    public GetInstrumentFullDetailUseCase(IInstrumentRepository instrumentRepository)
    {
        _instrumentRepository = instrumentRepository;
    }

    public async Task<InstrumentFullDetailDto> ExecuteAsync(InstrumentId instrumentId, CancellationToken cancellationToken)
    {
        var instrument = await _instrumentRepository.GetByIdAsync(instrumentId, cancellationToken)
            ?? throw new InstrumentNotFoundException(instrumentId);
        var scoringRules = await _instrumentRepository.GetScoringRulesAsync(instrumentId, cancellationToken);

        var questions = instrument.Questions
            .OrderBy(q => q.OrderNo)
            .Select(q => new FullQuestionDto(
                q.Id.Value,
                q.Text,
                q.OrderNo,
                q.Choices.OrderBy(c => c.OrderNo).Select(c => new FullChoiceDto(c.Id.Value, c.Label, c.Score, c.OrderNo)).ToList()))
            .ToList();

        var scoringRuleDtos = scoringRules
            .OrderBy(r => r.Range.Min)
            .Select(r => new FullScoringRuleDto(r.Id, r.Range.Min, r.Range.Max, r.Level, r.Interpretation, r.Advice))
            .ToList();

        return new InstrumentFullDetailDto(
            instrument.Id.Value,
            instrument.Code,
            instrument.Name,
            instrument.Version,
            instrument.Source,
            instrument.IsActive,
            questions,
            scoringRuleDtos);
    }
}
