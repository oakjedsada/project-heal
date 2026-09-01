namespace MindCheck.Application.Exceptions;

public sealed class FlowTransitionNotFoundException : Exception
{
    public int FlowTransitionId { get; }

    public FlowTransitionNotFoundException(int flowTransitionId)
        : base($"Flow transition {flowTransitionId} was not found.")
    {
        FlowTransitionId = flowTransitionId;
    }
}
