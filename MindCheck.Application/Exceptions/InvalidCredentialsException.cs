namespace MindCheck.Application.Exceptions;

// Fixed message on purpose — never distinguishes "no such user" from "wrong
// password", so the response can't be used to enumerate valid usernames.
public sealed class InvalidCredentialsException : Exception
{
    public InvalidCredentialsException()
        : base("Invalid username or password.")
    {
    }
}
