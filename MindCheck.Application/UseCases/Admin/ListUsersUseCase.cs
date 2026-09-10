using MindCheck.Application.Abstractions;
using MindCheck.Application.Dtos.Admin;

namespace MindCheck.Application.UseCases.Admin;

public sealed class ListUsersUseCase
{
    private readonly IUserRepository _userRepository;

    public ListUsersUseCase(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IReadOnlyList<UserDto>> ExecuteAsync(CancellationToken cancellationToken)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);

        return users
            .Select(u => new UserDto(u.Id.Value, u.Username, u.Email, u.Role.ToString(), u.CreatedAt))
            .ToList();
    }
}
