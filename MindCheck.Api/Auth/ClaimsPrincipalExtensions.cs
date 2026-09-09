using System.Security.Claims;
using MindCheck.Domain.ValueObjects;

namespace MindCheck.Api.Auth;

public static class ClaimsPrincipalExtensions
{
    public static UserId GetUserId(this ClaimsPrincipal principal)
    {
        var raw = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Authenticated principal is missing a NameIdentifier claim.");
        return new UserId(Guid.Parse(raw));
    }
}
