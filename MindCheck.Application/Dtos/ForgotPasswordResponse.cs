namespace MindCheck.Application.Dtos;

/// <summary>DevResetLink is null both when no account matched the email AND when a
/// real email was sent successfully — the response shape never reveals which
/// happened, so it can't be used to enumerate registered addresses. It's only
/// populated as a fallback when SMTP isn't configured (or a send failed), so
/// the flow still works end-to-end without real email delivery.</summary>
public sealed record ForgotPasswordResponse(string? DevResetLink);
