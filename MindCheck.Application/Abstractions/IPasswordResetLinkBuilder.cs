namespace MindCheck.Application.Abstractions;

public interface IPasswordResetLinkBuilder
{
    string BuildResetLink(string token);

    TimeSpan TokenLifetime { get; }
}
