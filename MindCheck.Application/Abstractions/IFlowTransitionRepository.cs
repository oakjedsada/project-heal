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
}
