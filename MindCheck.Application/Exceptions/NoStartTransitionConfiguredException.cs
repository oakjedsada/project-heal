namespace MindCheck.Application.Exceptions;

// Thrown when flow_transitions has no from_instrument_id = NULL row, meaning
// the seed data never defined an entry point into the assessment flow.
public sealed class NoStartTransitionConfiguredException : Exception
{
    public NoStartTransitionConfiguredException()
        : base("No session-start flow transition is configured.")
    {
    }
}
