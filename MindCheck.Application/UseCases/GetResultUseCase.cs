using MindCheck.Application.Abstractions;
using MindCheck.Application.Dtos;
using MindCheck.Application.Exceptions;
using MindCheck.Domain.ValueObjects;

namespace MindCheck.Application.UseCases;

public sealed class GetResultUseCase
{
    private readonly ISessionRepository _sessionRepository;
    private readonly IResultRepository _resultRepository;
    private readonly IInstrumentRepository _instrumentRepository;
    private readonly IHelpResourceProvider _helpResourceProvider;

    public GetResultUseCase(
        ISessionRepository sessionRepository,
        IResultRepository resultRepository,
        IInstrumentRepository instrumentRepository,
        IHelpResourceProvider helpResourceProvider)
    {
        _sessionRepository = sessionRepository;
        _resultRepository = resultRepository;
        _instrumentRepository = instrumentRepository;
        _helpResourceProvider = helpResourceProvider;
    }

    public async Task<ResultResponse> ExecuteAsync(UserId callerUserId, SessionId sessionId, CancellationToken cancellationToken)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken)
            ?? throw new SessionNotFoundException(sessionId);

        if (session.UserId != callerUserId)
        {
            throw new SessionAccessDeniedException(sessionId);
        }

        var results = await _resultRepository.GetBySessionIdAsync(sessionId, cancellationToken);

        var instrumentResults = new List<InstrumentResultDto>();
        foreach (var result in results.OrderBy(r => r.Id))
        {
            var instrument = await _instrumentRepository.GetByIdAsync(result.InstrumentId, cancellationToken)
                ?? throw new InstrumentNotFoundException(result.InstrumentId);
            var scoringRules = await _instrumentRepository.GetScoringRulesAsync(result.InstrumentId, cancellationToken);
            var matchingRule = scoringRules.FirstOrDefault(r => r.Level == result.Level);

            instrumentResults.Add(new InstrumentResultDto(
                instrument.Code,
                result.TotalScore,
                result.Level,
                matchingRule?.Interpretation ?? string.Empty,
                matchingRule?.Advice ?? string.Empty));
        }

        var isEmergency = session.State.Status == SessionStatus.Emergency;
        var isComplete = session.State.Status is SessionStatus.Completed or SessionStatus.Emergency;

        var nextAction = session.State.Status switch
        {
            SessionStatus.Emergency => "emergency",
            SessionStatus.Completed => "completed",
            _ => "in_progress"
        };

        return new ResultResponse(
            isComplete,
            nextAction,
            instrumentResults,
            isEmergency ? _helpResourceProvider.GetEmergencyResources().ToArray() : null);
    }
}
