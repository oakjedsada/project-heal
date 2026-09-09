using MindCheck.Application.Abstractions;
using MindCheck.Application.Dtos;
using MindCheck.Application.Exceptions;
using MindCheck.Domain.Entities;
using MindCheck.Domain.ValueObjects;

namespace MindCheck.Application.UseCases;

public sealed class StartSessionUseCase
{
    private readonly ISessionRepository _sessionRepository;
    private readonly IFlowTransitionRepository _flowTransitionRepository;
    private readonly TimeProvider _timeProvider;

    public StartSessionUseCase(
        ISessionRepository sessionRepository,
        IFlowTransitionRepository flowTransitionRepository,
        TimeProvider timeProvider)
    {
        _sessionRepository = sessionRepository;
        _flowTransitionRepository = flowTransitionRepository;
        _timeProvider = timeProvider;
    }

    public async Task<StartSessionResponse> ExecuteAsync(UserId userId, CancellationToken cancellationToken)
    {
        var startTransitions = await _flowTransitionRepository.GetByFromInstrumentIdAsync(null, cancellationToken);
        var startTransition = startTransitions.OrderBy(t => t.Id).FirstOrDefault()
            ?? throw new NoStartTransitionConfiguredException();

        var now = _timeProvider.GetUtcNow();
        var session = new Session(
            new SessionId(Guid.NewGuid()),
            userId,
            Guid.NewGuid().ToString("N"),
            now,
            now,
            SessionState.AtInstrument(startTransition.ToInstrumentId));

        await _sessionRepository.AddAsync(session, cancellationToken);

        return new StartSessionResponse(session.Id.Value);
    }
}
