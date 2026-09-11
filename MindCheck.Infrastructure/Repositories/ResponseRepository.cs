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

    public async Task<IReadOnlySet<QuestionId>> GetAnsweredQuestionIdsAsync(
        IReadOnlyList<QuestionId> questionIds, CancellationToken cancellationToken) =>
        (await _db.Responses
            .AsNoTracking()
            .Where(r => questionIds.Contains(r.QuestionId))
            .Select(r => r.QuestionId)
            .Distinct()
            .ToListAsync(cancellationToken))
        .ToHashSet();

    public async Task<IReadOnlySet<ChoiceId>> GetAnsweredChoiceIdsAsync(
        IReadOnlyList<ChoiceId> choiceIds, CancellationToken cancellationToken) =>
        (await _db.Responses
            .AsNoTracking()
            .Where(r => choiceIds.Contains(r.ChoiceId))
            .Select(r => r.ChoiceId)
            .Distinct()
            .ToListAsync(cancellationToken))
        .ToHashSet();
}
