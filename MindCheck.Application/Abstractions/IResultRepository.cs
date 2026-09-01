using MindCheck.Domain.Entities;
using MindCheck.Domain.ValueObjects;

namespace MindCheck.Application.Abstractions;

public interface IResultRepository
{
    Task AddAsync(Result result, CancellationToken cancellationToken);

    Task<IReadOnlyList<Result>> GetBySessionIdAsync(SessionId sessionId, CancellationToken cancellationToken);
}
