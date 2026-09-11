using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MindCheck.Application.Dtos.Admin;
using MindCheck.Application.UseCases.Admin;
using MindCheck.Domain.ValueObjects;

namespace MindCheck.Api.Controllers.Admin;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/instruments")]
public sealed class AdminInstrumentsController : ControllerBase
{
    private readonly CreateInstrumentUseCase _createInstrumentUseCase;
    private readonly ListInstrumentsUseCase _listInstrumentsUseCase;
    private readonly GetInstrumentDetailUseCase _getInstrumentDetailUseCase;
    private readonly GetInstrumentFullDetailUseCase _getInstrumentFullDetailUseCase;
    private readonly UpdateInstrumentUseCase _updateInstrumentUseCase;
    private readonly SetActiveInstrumentUseCase _setActiveInstrumentUseCase;

    public AdminInstrumentsController(
        CreateInstrumentUseCase createInstrumentUseCase,
        ListInstrumentsUseCase listInstrumentsUseCase,
        GetInstrumentDetailUseCase getInstrumentDetailUseCase,
        GetInstrumentFullDetailUseCase getInstrumentFullDetailUseCase,
        UpdateInstrumentUseCase updateInstrumentUseCase,
        SetActiveInstrumentUseCase setActiveInstrumentUseCase)
    {
        _createInstrumentUseCase = createInstrumentUseCase;
        _listInstrumentsUseCase = listInstrumentsUseCase;
        _getInstrumentDetailUseCase = getInstrumentDetailUseCase;
        _getInstrumentFullDetailUseCase = getInstrumentFullDetailUseCase;
        _updateInstrumentUseCase = updateInstrumentUseCase;
        _setActiveInstrumentUseCase = setActiveInstrumentUseCase;
    }

    [HttpPost]
    public async Task<ActionResult<CreateInstrumentResponse>> Create(
        [FromBody] CreateInstrumentRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _createInstrumentUseCase.ExecuteAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetDetail), new { id = response.InstrumentId }, response);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<InstrumentSummaryDto>>> List(CancellationToken cancellationToken)
    {
        return Ok(await _listInstrumentsUseCase.ExecuteAsync(cancellationToken));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<InstrumentDetailDto>> GetDetail(int id, CancellationToken cancellationToken)
    {
        return Ok(await _getInstrumentDetailUseCase.ExecuteAsync(new InstrumentId(id), cancellationToken));
    }

    [HttpGet("{id:int}/full")]
    public async Task<ActionResult<InstrumentFullDetailDto>> GetFullDetail(int id, CancellationToken cancellationToken)
    {
        return Ok(await _getInstrumentFullDetailUseCase.ExecuteAsync(new InstrumentId(id), cancellationToken));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<InstrumentFullDetailDto>> Update(
        int id,
        [FromBody] UpdateInstrumentRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _updateInstrumentUseCase.ExecuteAsync(new InstrumentId(id), request, cancellationToken));
    }

    [HttpPatch("{id:int}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activate(int id, CancellationToken cancellationToken)
    {
        await _setActiveInstrumentUseCase.ExecuteAsync(new InstrumentId(id), cancellationToken);
        return NoContent();
    }
}
