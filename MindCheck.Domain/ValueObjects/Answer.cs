namespace MindCheck.Domain.ValueObjects;

public readonly struct Answer
{
    public QuestionId QuestionId { get; }
    public ChoiceId ChoiceId { get; }
    public int Score { get; }

    public Answer(QuestionId questionId, ChoiceId choiceId, int score)
    {
        QuestionId = questionId;
        ChoiceId = choiceId;
        Score = score;
    }
}
