namespace MindCheck.Application.Exceptions;

public sealed class DuplicateUsernameException : Exception
{
    public string Username { get; }

    public DuplicateUsernameException(string username)
        : base($"Username '{username}' is already taken.")
    {
        Username = username;
    }
}
