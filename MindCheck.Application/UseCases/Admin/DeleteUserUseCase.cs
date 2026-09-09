using MindCheck.Application.Abstractions;
using MindCheck.Application.Exceptions;
using MindCheck.Domain.ValueObjects;

namespace MindCheck.Application.UseCases.Admin;

public sealed class DeleteUserUseCase
{
    private readonly IUserRepository _userRepository;

    public DeleteUserUseCase(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task ExecuteAsync(UserId id, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new UserNotFoundException(id);

        if (user.Role == UserRole.Admin
            && await _userRepository.CountByRoleAsync(UserRole.Admin, cancellationToken) <= 1)
        {
            throw new InvalidAdminRequestException("Cannot delete the last remaining admin.");
        }

        await _userRepository.DeleteAsync(id, cancellationToken);
    }
}
