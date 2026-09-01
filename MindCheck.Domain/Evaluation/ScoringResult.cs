namespace MindCheck.Domain.Evaluation;

public sealed class ScoringResult
{
    public int TotalScore { get; }
    public string Level { get; }
    public string Interpretation { get; }
    public string Advice { get; }

    public ScoringResult(int totalScore, string level, string interpretation, string advice)
    {
        TotalScore = totalScore;
        Level = level;
        Interpretation = interpretation;
        Advice = advice;
    }
}
