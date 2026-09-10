namespace MindCheck.Application.Exceptions;

public sealed class InvalidOrExpiredResetTokenException : Exception
{
    public InvalidOrExpiredResetTokenException()
        : base("This password reset link is invalid or has expired.")
    {
    }
}
