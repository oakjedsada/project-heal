using Microsoft.AspNetCore.Mvc;
using MindCheck.Application.Dtos;
using MindCheck.Application.UseCases;
using MindCheck.Domain.ValueObjects;

namespace MindCheck.Api.Controllers;

[ApiController]
[Route("api/sessions")]
public sealed class SessionsController : ControllerBase
{
    private readonly StartSessionUseCase _startSessionUseCase;
    private readonly GetNextQuestionUseCase _getNextQuestionUseCase;
    private readonly SubmitAnswerUseCase _submitAnswerUseCase;
    private readonly GetResultUseCase _getResultUseCase;

    public SessionsController(
        StartSessionUseCase startSessionUseCase,
        GetNextQuestionUseCase getNextQuestionUseCase,
        SubmitAnswerUseCase submitAnswerUseCase,
        GetResultUseCase getResultUseCase)
    {
        _startSessionUseCase = startSessionUseCase;
        _getNextQuestionUseCase = getNextQuestionUseCase;
        _submitAnswerUseCase = submitAnswerUseCase;
        _getResultUseCase = getResultUseCase;
    }

    [HttpPost]
    [ProducesResponseType(typeof(StartSessionResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<StartSessionResponse>> StartSession(CancellationToken cancellationToken)
    {
        var response = await _startSessionUseCase.ExecuteAsync(cancellationToken);
        return CreatedAtAction(nameof(GetResult), new { id = response.SessionId }, response);
    }

    [HttpGet("{id:guid}/next")]
    [ProducesResponseType(typeof(NextStepResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<NextStepResponse>> GetNext(Guid id, CancellationToken cancellationToken)
    {
        var response = await _getNextQuestionUseCase.ExecuteAsync(new SessionId(id), cancellationToken);
        return Ok(response);
    }

    [HttpPost("{id:guid}/answers")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SubmitAnswer(
        Guid id,
        [FromBody] SubmitAnswerRequest request,
        CancellationToken cancellationToken)
    {
        await _submitAnswerUseCase.ExecuteAsync(
            new SessionId(id),
            new QuestionId(request.QuestionId),
            new ChoiceId(request.ChoiceId),
            cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:guid}/result")]
    [ProducesResponseType(typeof(ResultResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ResultResponse>> GetResult(Guid id, CancellationToken cancellationToken)
    {
        var response = await _getResultUseCase.ExecuteAsync(new SessionId(id), cancellationToken);
        return Ok(response);
    }
}
