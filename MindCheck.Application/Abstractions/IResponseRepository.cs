using MindCheck.Domain.Entities;
using MindCheck.Domain.ValueObjects;

namespace MindCheck.Application.Abstractions;

public interface IResponseRepository
{
    Task<IReadOnlyList<Response>> GetBySessionIdAsync(SessionId sessionId, CancellationToken cancellationToken);

    Task AddAsync(Response response, CancellationToken cancellationToken);
}
