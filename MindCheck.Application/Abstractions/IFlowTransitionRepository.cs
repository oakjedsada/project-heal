using MindCheck.Domain.Entities;
using MindCheck.Domain.ValueObjects;

namespace MindCheck.Application.Abstractions;

public interface IFlowTransitionRepository
{
    /// <summary>Pass null to get the session-start transition(s).</summary>
    Task<IReadOnlyList<FlowTransition>> GetByFromInstrumentIdAsync(
        InstrumentId? fromInstrumentId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<FlowTransition>> GetAllAsync(CancellationToken cancellationToken);

    Task<FlowTransition> AddAsync(FlowTransition transition, CancellationToken cancellationToken);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);

    /// <summary>
    /// Atomically replaces every existing session-start transition (from = null)
    /// with a single new one pointing at <paramref name="toInstrumentId"/>, so
    /// exactly one instrument is ever the active starting point — see ADR 0016.
    /// </summary>
    Task<FlowTransition> SetStartTransitionAsync(InstrumentId toInstrumentId, CancellationToken cancellationToken);
}
