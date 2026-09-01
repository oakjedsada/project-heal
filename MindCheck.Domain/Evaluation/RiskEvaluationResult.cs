namespace MindCheck.Domain.Evaluation;

public sealed class RiskEvaluationResult
{
    public bool IsEscalated { get; }
    public IReadOnlyList<string> TriggeredActions { get; }

    public RiskEvaluationResult(bool isEscalated, IReadOnlyList<string> triggeredActions)
    {
        IsEscalated = isEscalated;
        TriggeredActions = triggeredActions;
    }
}
