using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace MindCheck.Tests.Integration;

// SMTP isn't configured in the test environment, so ForgotPasswordUseCase
// falls back to returning the reset link directly in the response instead of
// emailing it — same code path a real deploy without SMTP would take, so this
// still exercises the full token-issue -> validate -> consume lifecycle.
public sealed class PasswordResetTests : IClassFixture<MindCheckApiFactory>
{
    private readonly HttpClient _client;

    public PasswordResetTests(MindCheckApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ForgotPassword_ThenReset_AllowsLoginWithNewPasswordOnly()
    {
        var username = $"user-{Guid.NewGuid():N}";
        var email = $"{username}@example.com";
        await RegisterAsync(username, email, "correcthorsebattery");

        var forgotResponse = await _client.PostAsJsonAsync("/api/auth/forgot-password", new { email });
        forgotResponse.EnsureSuccessStatusCode();
        var forgotBody = await forgotResponse.Content.ReadFromJsonAsync<JsonElement>();
        var resetLink = forgotBody.GetProperty("devResetLink").GetString();
        resetLink.Should().NotBeNullOrEmpty("SMTP isn't configured in tests, so the link should come back directly");

        var token = new Uri(resetLink!).Query.Split("token=")[1];

        var resetResponse = await _client.PostAsJsonAsync(
            "/api/auth/reset-password",
            new { token, newPassword = "brandNewPassword123" });
        resetResponse.EnsureSuccessStatusCode();

        var oldPasswordLogin = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new { usernameOrEmail = username, password = "correcthorsebattery" });
        oldPasswordLogin.IsSuccessStatusCode.Should().BeFalse();

        var newPasswordLogin = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new { usernameOrEmail = username, password = "brandNewPassword123" });
        newPasswordLogin.EnsureSuccessStatusCode();

        // The token is single-use — replaying it must not work a second time.
        var replay = await _client.PostAsJsonAsync(
            "/api/auth/reset-password",
            new { token, newPassword = "yetAnotherPassword123" });
        replay.IsSuccessStatusCode.Should().BeFalse();
    }

    [Fact]
    public async Task ForgotPassword_WithUnknownEmail_ReturnsSameShapeAsSuccess()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/forgot-password", new { email = "nobody-here@example.com" });

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("devResetLink").ValueKind.Should().Be(JsonValueKind.Null);
    }

    [Fact]
    public async Task ResetPassword_WithInvalidToken_Fails()
    {
        var response = await _client.PostAsJsonAsync(
            "/api/auth/reset-password",
            new { token = "not-a-real-token", newPassword = "whatever12345" });

        response.IsSuccessStatusCode.Should().BeFalse();
    }

    private async Task RegisterAsync(string username, string email, string password)
    {
        var response = await _client.PostAsJsonAsync("/api/auth/register", new { username, email, password });
        response.EnsureSuccessStatusCode();
    }
}
