namespace MindCheck.Application.Dtos;

public sealed record ResetPasswordRequest(string Token, string NewPassword);
