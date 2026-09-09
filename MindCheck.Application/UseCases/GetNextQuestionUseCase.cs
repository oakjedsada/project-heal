using MindCheck.Application.Abstractions;
using MindCheck.Application.Dtos;
using MindCheck.Application.Exceptions;
using MindCheck.Domain.ValueObjects;

namespace MindCheck.Application.UseCases;

public sealed class GetNextQuestionUseCase
{
    private readonly ISessionRepository _sessionRepository;
    private readonly IInstrumentRepository _instrumentRepository;
    private readonly IResponseRepository _responseRepository;

    public GetNextQuestionUseCase(
        ISessionRepository sessionRepository,
        IInstrumentRepository instrumentRepository,
        IResponseRepository responseRepository)
    {
        _sessionRepository = sessionRepository;
        _instrumentRepository = instrumentRepository;
        _responseRepository = responseRepository;
    }

    public async Task<NextStepResponse> ExecuteAsync(UserId callerUserId, SessionId sessionId, CancellationToken cancellationToken)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken)
            ?? throw new SessionNotFoundException(sessionId);

        if (session.UserId != callerUserId)
        {
            throw new SessionAccessDeniedException(sessionId);
        }

        if (session.State.CurrentInstrumentId is not { } currentInstrumentId)
        {
            return new NextStepResponse(IsComplete: true, Question: null);
        }

        var instrument = await _instrumentRepository.GetByIdAsync(currentInstrumentId, cancellationToken)
            ?? throw new InstrumentNotFoundException(currentInstrumentId);

        var responses = await _responseRepository.GetBySessionIdAsync(sessionId, cancellationToken);
        var answeredQuestionIds = responses.Select(r => r.QuestionId).ToHashSet();

        var nextQuestion = instrument.Questions
            .Where(q => !answeredQuestionIds.Contains(q.Id))
            .OrderBy(q => q.OrderNo)
            .FirstOrDefault();

        if (nextQuestion is null)
        {
            return new NextStepResponse(IsComplete: true, Question: null);
        }

        var dto = new QuestionDto(
            nextQuestion.Id.Value,
            nextQuestion.Text,
            nextQuestion.Choices
                .OrderBy(c => c.OrderNo)
                .Select(c => new ChoiceDto(c.Id.Value, c.Label, c.OrderNo))
                .ToList());

        return new NextStepResponse(IsComplete: false, Question: dto);
    }
}
