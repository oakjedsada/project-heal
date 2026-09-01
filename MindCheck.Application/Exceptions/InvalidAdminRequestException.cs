namespace MindCheck.Application.Exceptions;

// Generic validation failure for admin write requests (bad enum text, a risk
// rule pointing at a question order number that wasn't submitted, etc.) —
// always a client input problem, mapped to 400 at the Api layer.
public sealed class InvalidAdminRequestException : Exception
{
    public InvalidAdminRequestException(string message)
        : base(message)
    {
    }
}
