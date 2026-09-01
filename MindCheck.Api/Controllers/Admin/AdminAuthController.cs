using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MindCheck.Api.Auth;

namespace MindCheck.Api.Controllers.Admin;

public sealed record AdminLoginRequest(string Password);

public sealed record AdminLoginResponse(string Token);

[ApiController]
[Route("api/admin/auth")]
public sealed class AdminAuthController : ControllerBase
{
    private readonly AdminAuthOptions _options;

    public AdminAuthController(IOptions<AdminAuthOptions> options)
    {
        _options = options.Value;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AdminLoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<AdminLoginResponse> Login([FromBody] AdminLoginRequest request)
    {
        if (string.IsNullOrEmpty(_options.Password) || request.Password != _options.Password)
        {
            return Unauthorized();
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.JwtSigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[] { new Claim(ClaimTypes.Role, "admin") };

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_options.TokenLifetimeMinutes),
            signingCredentials: credentials);

        return Ok(new AdminLoginResponse(new JwtSecurityTokenHandler().WriteToken(token)));
    }
}
