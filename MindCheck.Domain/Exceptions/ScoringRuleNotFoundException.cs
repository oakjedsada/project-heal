namespace MindCheck.Domain.Exceptions;

public sealed class ScoringRuleNotFoundException : Exception
{
    public int TotalScore { get; }

    public ScoringRuleNotFoundException(int totalScore)
        : base($"No scoring rule covers total score {totalScore}.")
    {
        TotalScore = totalScore;
    }
}
