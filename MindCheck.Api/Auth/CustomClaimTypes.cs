namespace MindCheck.Api.Auth;

public static class CustomClaimTypes
{
    // Carries User.TokenVersion so an already-issued JWT can be revoked by
    // bumping the DB value — see JwtAuthTokenGenerator and Program.cs's
    // OnTokenValidated handler.
    public const string TokenVersion = "tv";
}
