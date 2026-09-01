using MindCheck.Domain.Entities;
using MindCheck.Domain.ValueObjects;

namespace MindCheck.Domain.Evaluation;

public interface IAssessmentFlowEngine
{
    /// <summary>
    /// Decides the next step after one instrument's answers have been scored.
    /// <paramref name="candidateTransitions"/> must already be filtered to the
    /// instrument that was just completed (or to the session-start sentinel).
    /// </summary>
    FlowDecision Decide(
        RiskEvaluationResult riskResult,
        ScoringResult scoringResult,
        IReadOnlyCollection<Answer> answers,
        IReadOnlyCollection<FlowTransition> candidateTransitions);
}
