using MindCheck.Domain.ValueObjects;

namespace MindCheck.Domain.Entities;

public sealed class Response
{
    public int Id { get; }
    public SessionId SessionId { get; }
    public QuestionId QuestionId { get; }
    public ChoiceId ChoiceId { get; }
    public DateTimeOffset AnsweredAt { get; }

    public Response(int id, SessionId sessionId, QuestionId questionId, ChoiceId choiceId, DateTimeOffset answeredAt)
    {
        Id = id;
        SessionId = sessionId;
        QuestionId = questionId;
        ChoiceId = choiceId;
        AnsweredAt = answeredAt;
    }
}
