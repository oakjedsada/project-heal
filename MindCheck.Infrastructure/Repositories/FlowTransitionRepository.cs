using Microsoft.EntityFrameworkCore;
using MindCheck.Application.Abstractions;
using MindCheck.Domain.Entities;
using MindCheck.Domain.ValueObjects;

namespace MindCheck.Infrastructure.Repositories;

public sealed class FlowTransitionRepository : IFlowTransitionRepository
{
    private readonly MindCheckDbContext _db;

    public FlowTransitionRepository(MindCheckDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<FlowTransition>> GetByFromInstrumentIdAsync(
        InstrumentId? fromInstrumentId,
        CancellationToken cancellationToken) =>
        await _db.FlowTransitions
            .AsNoTracking()
            .Where(t => t.FromInstrumentId == fromInstrumentId)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<FlowTransition>> GetAllAsync(CancellationToken cancellationToken) =>
        await _db.FlowTransitions.AsNoTracking().ToListAsync(cancellationToken);

    public async Task<FlowTransition> AddAsync(FlowTransition transition, CancellationToken cancellationToken)
    {
        _db.FlowTransitions.Add(transition);
        await _db.SaveChangesAsync(cancellationToken);
        return transition;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var rowsAffected = await _db.FlowTransitions
            .Where(t => t.Id == id)
            .ExecuteDeleteAsync(cancellationToken);
        return rowsAffected > 0;
    }

    public async Task<FlowTransition> SetStartTransitionAsync(InstrumentId toInstrumentId, CancellationToken cancellationToken)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

        await _db.FlowTransitions
            .Where(t => t.FromInstrumentId == null)
            .ExecuteDeleteAsync(cancellationToken);

        var transition = new FlowTransition(0, null, FlowConditionType.Always, null, null, toInstrumentId);
        _db.FlowTransitions.Add(transition);
        await _db.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return transition;
    }
}
