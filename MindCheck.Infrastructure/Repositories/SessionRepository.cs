using Microsoft.EntityFrameworkCore;
using MindCheck.Application.Abstractions;
using MindCheck.Domain.Entities;
using MindCheck.Domain.ValueObjects;

namespace MindCheck.Infrastructure.Repositories;

public sealed class SessionRepository : ISessionRepository
{
    private readonly MindCheckDbContext _db;

    public SessionRepository(MindCheckDbContext db)
    {
        _db = db;
    }

    public Task<Session?> GetByIdAsync(SessionId id, CancellationToken cancellationToken) =>
        _db.Sessions.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task AddAsync(Session session, CancellationToken cancellationToken)
    {
        _db.Sessions.Add(session);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Session session, CancellationToken cancellationToken)
    {
        _db.Sessions.Update(session);
        await _db.SaveChangesAsync(cancellationToken);
    }
}
