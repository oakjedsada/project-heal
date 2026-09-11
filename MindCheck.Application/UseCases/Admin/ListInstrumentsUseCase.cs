using MindCheck.Application.Abstractions;
using MindCheck.Application.Dtos.Admin;

namespace MindCheck.Application.UseCases.Admin;

public sealed class ListInstrumentsUseCase
{
    private readonly IInstrumentRepository _instrumentRepository;
    private readonly IFlowTransitionRepository _flowTransitionRepository;

    public ListInstrumentsUseCase(
        IInstrumentRepository instrumentRepository,
        IFlowTransitionRepository flowTransitionRepository)
    {
        _instrumentRepository = instrumentRepository;
        _flowTransitionRepository = flowTransitionRepository;
    }

    public async Task<IReadOnlyList<InstrumentSummaryDto>> ExecuteAsync(CancellationToken cancellationToken)
    {
        var instruments = await _instrumentRepository.GetAllAsync(cancellationToken);

        // Mirrors StartSessionUseCase's own "first by Id" pick — after
        // SetActiveInstrumentUseCase runs, there's always exactly one of
        // these, but this stays consistent even if that invariant is ever
        // violated by a direct DB edit.
        var startTransitions = await _flowTransitionRepository.GetByFromInstrumentIdAsync(null, cancellationToken);
        var activeStartInstrumentId = startTransitions.OrderBy(t => t.Id).FirstOrDefault()?.ToInstrumentId;

        return instruments
            .OrderBy(i => i.Id.Value)
            .Select(i => new InstrumentSummaryDto(i.Id.Value, i.Code, i.Name, i.Id == activeStartInstrumentId))
            .ToList();
    }
}
