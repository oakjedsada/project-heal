using MindCheck.Application.Abstractions;
using MindCheck.Application.Exceptions;
using MindCheck.Domain.ValueObjects;

namespace MindCheck.Application.UseCases.Admin;

// Lets an admin pick which instrument every new session starts with, without
// having to manually delete/recreate flow_transitions rows themselves — see
// ADR 0016.
public sealed class SetActiveInstrumentUseCase
{
    private readonly IInstrumentRepository _instrumentRepository;
    private readonly IFlowTransitionRepository _flowTransitionRepository;

    public SetActiveInstrumentUseCase(
        IInstrumentRepository instrumentRepository,
        IFlowTransitionRepository flowTransitionRepository)
    {
        _instrumentRepository = instrumentRepository;
        _flowTransitionRepository = flowTransitionRepository;
    }

    public async Task ExecuteAsync(InstrumentId instrumentId, CancellationToken cancellationToken)
    {
        _ = await _instrumentRepository.GetByIdAsync(instrumentId, cancellationToken)
            ?? throw new InstrumentNotFoundException(instrumentId);

        await _flowTransitionRepository.SetStartTransitionAsync(instrumentId, cancellationToken);
    }
}
