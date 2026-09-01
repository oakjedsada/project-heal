using MindCheck.Domain.ValueObjects;

namespace MindCheck.Domain.Entities;

public sealed class Choice
{
    public ChoiceId Id { get; }
    public QuestionId QuestionId { get; }
    public string Label { get; }
    public int Score { get; }
    public int OrderNo { get; }

    public Choice(ChoiceId id, QuestionId questionId, string label, int score, int orderNo)
    {
        Id = id;
        QuestionId = questionId;
        Label = label;
        Score = score;
        OrderNo = orderNo;
    }
}
