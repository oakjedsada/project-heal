using MindCheck.Domain.ValueObjects;

namespace MindCheck.Domain.Entities;

public sealed class Choice
{
    public ChoiceId Id { get; }
    public QuestionId QuestionId { get; }
    public string Label { get; private set; }
    public int Score { get; private set; }
    public int OrderNo { get; private set; }

    public Choice(ChoiceId id, QuestionId questionId, string label, int score, int orderNo)
    {
        Id = id;
        QuestionId = questionId;
        Label = label;
        Score = score;
        OrderNo = orderNo;
    }

    public void ChangeDetails(string label, int score, int orderNo)
    {
        Label = label;
        Score = score;
        OrderNo = orderNo;
    }
}
