namespace MindCheck.Application.Dtos;

/// <summary>UsernameOrEmail: the login field accepts either, so the caller isn't
/// forced to remember which one they registered with.</summary>
public sealed record LoginRequest(string UsernameOrEmail, string Password);
