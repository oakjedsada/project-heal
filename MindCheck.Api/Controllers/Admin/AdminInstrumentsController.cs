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

    public AdminInstrumentsController(
        CreateInstrumentUseCase createInstrumentUseCase,
        ListInstrumentsUseCase listInstrumentsUseCase,
        GetInstrumentDetailUseCase getInstrumentDetailUseCase)
    {
        _createInstrumentUseCase = createInstrumentUseCase;
        _listInstrumentsUseCase = listInstrumentsUseCase;
        _getInstrumentDetailUseCase = getInstrumentDetailUseCase;
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
}
