using MindCheck.Domain.Entities;
using MindCheck.Domain.ValueObjects;

namespace MindCheck.Application.Abstractions;

public interface IResponseRepository
{
    Task<IReadOnlyList<Response>> GetBySessionIdAsync(SessionId sessionId, CancellationToken cancellationToken);

    Task AddAsync(Response response, CancellationToken cancellationToken);

    /// <summary>Of the given questions, which ones have at least one recorded answer — used to block deleting a question/choice a real user already answered.</summary>
    Task<IReadOnlySet<QuestionId>> GetAnsweredQuestionIdsAsync(IReadOnlyList<QuestionId> questionIds, CancellationToken cancellationToken);

    Task<IReadOnlySet<ChoiceId>> GetAnsweredChoiceIdsAsync(IReadOnlyList<ChoiceId> choiceIds, CancellationToken cancellationToken);
}
