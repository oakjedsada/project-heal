using MindCheck.Application.Abstractions;
using MindCheck.Application.Exceptions;

namespace MindCheck.Application.UseCases.Admin;

public sealed class DeleteFlowTransitionUseCase
{
    private readonly IFlowTransitionRepository _flowTransitionRepository;

    public DeleteFlowTransitionUseCase(IFlowTransitionRepository flowTransitionRepository)
    {
        _flowTransitionRepository = flowTransitionRepository;
    }

    public async Task ExecuteAsync(int id, CancellationToken cancellationToken)
    {
        var deleted = await _flowTransitionRepository.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            throw new FlowTransitionNotFoundException(id);
        }
    }
}
