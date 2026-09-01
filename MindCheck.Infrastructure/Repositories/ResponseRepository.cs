using Microsoft.EntityFrameworkCore;
using MindCheck.Application.Abstractions;
using MindCheck.Domain.Entities;
using MindCheck.Domain.ValueObjects;

namespace MindCheck.Infrastructure.Repositories;

public sealed class ResponseRepository : IResponseRepository
{
    private readonly MindCheckDbContext _db;

    public ResponseRepository(MindCheckDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<Response>> GetBySessionIdAsync(SessionId sessionId, CancellationToken cancellationToken) =>
        await _db.Responses.AsNoTracking().Where(r => r.SessionId == sessionId).ToListAsync(cancellationToken);

    public async Task AddAsync(Response response, CancellationToken cancellationToken)
    {
        _db.Responses.Add(response);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
