using Microsoft.AspNetCore.Identity;
using MindCheck.Application.Abstractions;
using MindCheck.Domain.Entities;

namespace MindCheck.Api.Auth;

// Wraps ASP.NET Core Identity's battle-tested PasswordHasher behind the
// Application-owned IPasswordHasher interface, so Application never
// references Microsoft.AspNetCore.Identity directly. The hasher's TUser
// argument is only used by custom IPasswordHasher<TUser> overrides to
// influence hashing per-user; the default implementation ignores it, so
// passing null! here (headless usage) is safe.
public sealed class PasswordHasherAdapter : IPasswordHasher
{
    private readonly PasswordHasher<User> _inner = new();

    public string Hash(string password) => _inner.HashPassword(null!, password);

    public bool Verify(string password, string passwordHash) =>
        _inner.VerifyHashedPassword(null!, passwordHash, password) != PasswordVerificationResult.Failed;
}
