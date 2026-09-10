namespace MindCheck.Application.Abstractions;

public interface IEmailSender
{
    /// <summary>Returns true if an email was actually sent (SMTP configured and the
    /// send succeeded), false otherwise — callers use false as the signal to fall
    /// back to some other way of delivering the link (e.g. showing it in-app).</summary>
    Task<bool> TrySendPasswordResetEmailAsync(string toEmail, string resetLink, CancellationToken cancellationToken);
}
