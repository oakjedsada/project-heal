using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MindCheck.Application.Dtos.Admin;
using MindCheck.Application.UseCases.Admin;

namespace MindCheck.Api.Controllers.Admin;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/flow-transitions")]
public sealed class AdminFlowTransitionsController : ControllerBase
{
    private readonly CreateFlowTransitionUseCase _createFlowTransitionUseCase;
    private readonly ListFlowTransitionsUseCase _listFlowTransitionsUseCase;
    private readonly DeleteFlowTransitionUseCase _deleteFlowTransitionUseCase;

    public AdminFlowTransitionsController(
        CreateFlowTransitionUseCase createFlowTransitionUseCase,
        ListFlowTransitionsUseCase listFlowTransitionsUseCase,
        DeleteFlowTransitionUseCase deleteFlowTransitionUseCase)
    {
        _createFlowTransitionUseCase = createFlowTransitionUseCase;
        _listFlowTransitionsUseCase = listFlowTransitionsUseCase;
        _deleteFlowTransitionUseCase = deleteFlowTransitionUseCase;
    }

    [HttpPost]
    public async Task<ActionResult<FlowTransitionDto>> Create(
        [FromBody] CreateFlowTransitionRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _createFlowTransitionUseCase.ExecuteAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<FlowTransitionDto>>> List(CancellationToken cancellationToken)
    {
        return Ok(await _listFlowTransitionsUseCase.ExecuteAsync(cancellationToken));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _deleteFlowTransitionUseCase.ExecuteAsync(id, cancellationToken);
        return NoContent();
    }
}
