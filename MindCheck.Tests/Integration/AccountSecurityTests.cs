using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace MindCheck.Tests.Integration;

// Covers the three auth-hardening mechanisms added on top of ADR 0014's
// unified accounts: failed-login lockout, per-IP rate limiting on the
// unauthenticated auth endpoints, and JWT revocation via TokenVersion — see
// ADR 0015. Each test spins up its own customized WebApplicationFactory via
// WithWebHostBuilder so its threshold overrides (and the requests needed to
// trip them) never interfere with each other or with the other test classes
// sharing MindCheckApiFactory's default config.
public sealed class AccountSecurityTests : IClassFixture<MindCheckApiFactory>
{
    private readonly MindCheckApiFactory _factory;

    public AccountSecurityTests(MindCheckApiFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Login_AfterExceedingFailedAttempts_LocksAccountEvenWithCorrectPassword()
    {
        using var client = _factory
            .WithWebHostBuilder(builder => builder.ConfigureAppConfiguration((_, configBuilder) =>
                configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Auth:MaxFailedLoginAttempts"] = "2",
                })))
            .CreateClient();

        var username = $"lockout-{Guid.NewGuid():N}";
        var email = $"{username}@example.com";
        const string password = "correcthorsebattery";

        (await client.PostAsJsonAsync("/api/auth/register", new { username, email, password }))
            .EnsureSuccessStatusCode();

        for (var attempt = 0; attempt < 2; attempt++)
        {
            var wrongAttempt = await client.PostAsJsonAsync(
                "/api/auth/login",
                new { usernameOrEmail = username, password = "wrong-password" });
            wrongAttempt.IsSuccessStatusCode.Should().BeFalse();
        }

        // Threshold (2) is now reached — even the correct password must be
        // rejected while the lockout window is active.
        var lockedOutAttempt = await client.PostAsJsonAsync(
            "/api/auth/login",
            new { usernameOrEmail = username, password });
        ((int)lockedOutAttempt.StatusCode).Should().Be(423);
    }

    [Fact]
    public async Task Login_ExceedingRateLimit_Returns429()
    {
        using var client = _factory
            .WithWebHostBuilder(builder => builder.ConfigureAppConfiguration((_, configBuilder) =>
                configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Auth:AuthRateLimitPermitLimit"] = "3",
                    ["Auth:AuthRateLimitWindowSeconds"] = "60",
                })))
            .CreateClient();

        var usernameOrEmail = $"rate-limit-{Guid.NewGuid():N}";

        for (var i = 0; i < 3; i++)
        {
            await client.PostAsJsonAsync("/api/auth/login", new { usernameOrEmail, password = "whatever" });
        }

        var overLimitAttempt = await client.PostAsJsonAsync(
            "/api/auth/login",
            new { usernameOrEmail, password = "whatever" });
        ((int)overLimitAttempt.StatusCode).Should().Be(429);
    }

    [Fact]
    public async Task LogoutAll_RevokesPreviouslyIssuedToken()
    {
        var client = _factory.CreateClient();

        var username = $"revoke-{Guid.NewGuid():N}";
        var email = $"{username}@example.com";
        const string password = "correcthorsebattery";

        var registerResponse = await client.PostAsJsonAsync("/api/auth/register", new { username, email, password });
        registerResponse.EnsureSuccessStatusCode();
        var token = (await registerResponse.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("token").GetString();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // The freshly issued token can hit an authenticated endpoint.
        var beforeRevoke = await client.PostAsync("/api/sessions", null);
        beforeRevoke.IsSuccessStatusCode.Should().BeTrue();

        (await client.PostAsync("/api/auth/logout-all", null)).EnsureSuccessStatusCode();

        // Same token, now stale (TokenVersion bumped) — must be rejected.
        var afterRevoke = await client.PostAsync("/api/sessions", null);
        ((int)afterRevoke.StatusCode).Should().Be(401);
    }
}
