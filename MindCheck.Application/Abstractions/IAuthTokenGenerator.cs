using MindCheck.Domain.ValueObjects;

namespace MindCheck.Application.Abstractions;

public interface IAuthTokenGenerator
{
    string GenerateToken(UserId userId, string username, UserRole role, int tokenVersion);
}
