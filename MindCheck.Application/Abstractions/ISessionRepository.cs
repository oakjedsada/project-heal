using MindCheck.Domain.Entities;
using MindCheck.Domain.ValueObjects;

namespace MindCheck.Application.Abstractions;

public interface ISessionRepository
{
    Task<Session?> GetByIdAsync(SessionId id, CancellationToken cancellationToken);

    Task AddAsync(Session session, CancellationToken cancellationToken);

    Task UpdateAsync(Session session, CancellationToken cancellationToken);

    /// <summary>Anonymous aggregate: every session's start time, nothing session-identifying.</summary>
    Task<IReadOnlyList<DateTimeOffset>> GetAllStartedAtAsync(CancellationToken cancellationToken);
}
