using Microsoft.EntityFrameworkCore;
using MindCheck.Application.Abstractions;
using MindCheck.Domain.Entities;
using MindCheck.Domain.ValueObjects;

namespace MindCheck.Infrastructure.Repositories;

public sealed class ResultRepository : IResultRepository
{
    private readonly MindCheckDbContext _db;

    public ResultRepository(MindCheckDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(Result result, CancellationToken cancellationToken)
    {
        _db.Results.Add(result);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Result>> GetBySessionIdAsync(SessionId sessionId, CancellationToken cancellationToken) =>
        await _db.Results.AsNoTracking().Where(r => r.SessionId == sessionId).ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<InstrumentLevelCount>> GetLevelCountsAsync(CancellationToken cancellationToken) =>
        await _db.Results
            .AsNoTracking()
            .GroupBy(r => new { r.InstrumentId, r.Level })
            .Select(g => new InstrumentLevelCount(g.Key.InstrumentId, g.Key.Level, g.Count()))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlySet<string>> GetLevelsInUseAsync(InstrumentId instrumentId, CancellationToken cancellationToken) =>
        (await _db.Results
            .AsNoTracking()
            .Where(r => r.InstrumentId == instrumentId)
            .Select(r => r.Level)
            .Distinct()
            .ToListAsync(cancellationToken))
        .ToHashSet();
}
