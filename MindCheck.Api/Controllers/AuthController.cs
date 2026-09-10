using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MindCheck.Application.Dtos;
using MindCheck.Application.UseCases;

namespace MindCheck.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly RegisterUserUseCase _registerUserUseCase;
    private readonly LoginUseCase _loginUseCase;
    private readonly ForgotPasswordUseCase _forgotPasswordUseCase;
    private readonly ResetPasswordUseCase _resetPasswordUseCase;

    public AuthController(
        RegisterUserUseCase registerUserUseCase,
        LoginUseCase loginUseCase,
        ForgotPasswordUseCase forgotPasswordUseCase,
        ResetPasswordUseCase resetPasswordUseCase)
    {
        _registerUserUseCase = registerUserUseCase;
        _loginUseCase = loginUseCase;
        _forgotPasswordUseCase = forgotPasswordUseCase;
        _resetPasswordUseCase = resetPasswordUseCase;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthTokenResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthTokenResponse>> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _registerUserUseCase.ExecuteAsync(request, cancellationToken));
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthTokenResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _loginUseCase.ExecuteAsync(request, cancellationToken));
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ForgotPasswordResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ForgotPasswordResponse>> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _forgotPasswordUseCase.ExecuteAsync(request, cancellationToken));
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        await _resetPasswordUseCase.ExecuteAsync(request, cancellationToken);
        return NoContent();
    }
}
