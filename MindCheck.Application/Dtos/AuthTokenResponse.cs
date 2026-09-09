namespace MindCheck.Application.Dtos;

public sealed record AuthTokenResponse(string Token, Guid UserId, string Username, string Role);
