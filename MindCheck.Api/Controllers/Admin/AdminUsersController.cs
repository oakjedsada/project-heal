using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MindCheck.Application.Dtos.Admin;
using MindCheck.Application.UseCases.Admin;
using MindCheck.Domain.ValueObjects;

namespace MindCheck.Api.Controllers.Admin;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/users")]
public sealed class AdminUsersController : ControllerBase
{
    private readonly ListUsersUseCase _listUsersUseCase;
    private readonly CreateUserUseCase _createUserUseCase;
    private readonly ChangeUserRoleUseCase _changeUserRoleUseCase;
    private readonly DeleteUserUseCase _deleteUserUseCase;

    public AdminUsersController(
        ListUsersUseCase listUsersUseCase,
        CreateUserUseCase createUserUseCase,
        ChangeUserRoleUseCase changeUserRoleUseCase,
        DeleteUserUseCase deleteUserUseCase)
    {
        _listUsersUseCase = listUsersUseCase;
        _createUserUseCase = createUserUseCase;
        _changeUserRoleUseCase = changeUserRoleUseCase;
        _deleteUserUseCase = deleteUserUseCase;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> List(CancellationToken cancellationToken)
    {
        return Ok(await _listUsersUseCase.ExecuteAsync(cancellationToken));
    }

    [HttpPost]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _createUserUseCase.ExecuteAsync(request, cancellationToken));
    }

    [HttpPatch("{id:guid}/role")]
    public async Task<ActionResult<UserDto>> ChangeRole(Guid id, [FromBody] ChangeUserRoleRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _changeUserRoleUseCase.ExecuteAsync(new UserId(id), request, cancellationToken));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _deleteUserUseCase.ExecuteAsync(new UserId(id), cancellationToken);
        return NoContent();
    }
}
