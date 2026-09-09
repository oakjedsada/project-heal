namespace MindCheck.Application.Exceptions;

// Validation failure for the public register/login endpoints (blank
// username, too-short password) — kept separate from
// InvalidAdminRequestException so a 400 here isn't misleadingly labeled
// "admin".
public sealed class InvalidAuthRequestException : Exception
{
    public InvalidAuthRequestException(string message)
        : base(message)
    {
    }
}
