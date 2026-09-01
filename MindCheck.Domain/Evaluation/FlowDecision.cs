using MindCheck.Domain.ValueObjects;

namespace MindCheck.Domain.Evaluation;

public enum FlowOutcome
{
    Emergency,
    Completed,
    ContinueToNextInstrument
}

public sealed class FlowDecision
{
    public FlowOutcome Outcome { get; }
    public InstrumentId? NextInstrumentId { get; }

    private FlowDecision(FlowOutcome outcome, InstrumentId? nextInstrumentId)
    {
        Outcome = outcome;
        NextInstrumentId = nextInstrumentId;
    }

    public static FlowDecision Emergency() => new(FlowOutcome.Emergency, null);

    public static FlowDecision Completed() => new(FlowOutcome.Completed, null);

    public static FlowDecision ContinueTo(InstrumentId nextInstrumentId) =>
        new(FlowOutcome.ContinueToNextInstrument, nextInstrumentId);
}
