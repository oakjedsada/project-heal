using MindCheck.Application.Abstractions;
using MindCheck.Application.Exceptions;
using MindCheck.Domain.Entities;
using MindCheck.Domain.Evaluation;
using MindCheck.Domain.ValueObjects;

namespace MindCheck.Application.UseCases;

public sealed class SubmitAnswerUseCase
{
    private readonly ISessionRepository _sessionRepository;
    private readonly IInstrumentRepository _instrumentRepository;
    private readonly IResponseRepository _responseRepository;
    private readonly IResultRepository _resultRepository;
    private readonly IFlowTransitionRepository _flowTransitionRepository;
    private readonly IScoringEvaluator _scoringEvaluator;
    private readonly IRiskEvaluator _riskEvaluator;
    private readonly IAssessmentFlowEngine _flowEngine;
    private readonly TimeProvider _timeProvider;

    public SubmitAnswerUseCase(
        ISessionRepository sessionRepository,
        IInstrumentRepository instrumentRepository,
        IResponseRepository responseRepository,
        IResultRepository resultRepository,
        IFlowTransitionRepository flowTransitionRepository,
        IScoringEvaluator scoringEvaluator,
        IRiskEvaluator riskEvaluator,
        IAssessmentFlowEngine flowEngine,
        TimeProvider timeProvider)
    {
        _sessionRepository = sessionRepository;
        _instrumentRepository = instrumentRepository;
        _responseRepository = responseRepository;
        _resultRepository = resultRepository;
        _flowTransitionRepository = flowTransitionRepository;
        _scoringEvaluator = scoringEvaluator;
        _riskEvaluator = riskEvaluator;
        _flowEngine = flowEngine;
        _timeProvider = timeProvider;
    }

    public async Task ExecuteAsync(
        UserId callerUserId,
        SessionId sessionId,
        QuestionId questionId,
        ChoiceId choiceId,
        CancellationToken cancellationToken)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken)
            ?? throw new SessionNotFoundException(sessionId);

        if (session.UserId != callerUserId)
        {
            throw new SessionAccessDeniedException(sessionId);
        }

        if (session.State.CurrentInstrumentId is not { } currentInstrumentId)
        {
            throw new SessionAlreadyFinishedException(sessionId);
        }

        var instrument = await _instrumentRepository.GetByIdAsync(currentInstrumentId, cancellationToken)
            ?? throw new InstrumentNotFoundException(currentInstrumentId);

        var question = instrument.Questions.FirstOrDefault(q => q.Id == questionId)
            ?? throw new QuestionNotInCurrentInstrumentException(questionId, currentInstrumentId);

        _ = question.Choices.FirstOrDefault(c => c.Id == choiceId)
            ?? throw new ChoiceNotFoundException(choiceId, questionId);

        var answeredAt = _timeProvider.GetUtcNow();
        await _responseRepository.AddAsync(new Response(0, sessionId, questionId, choiceId, answeredAt), cancellationToken);

        var answers = await TryBuildCompletedInstrumentAnswersAsync(sessionId, instrument, cancellationToken);
        if (answers is null)
        {
            return;
        }

        var expectedQuestionIds = instrument.Questions.Select(q => q.Id).ToList();
        var scoringRules = await _instrumentRepository.GetScoringRulesAsync(currentInstrumentId, cancellationToken);
        var riskRules = await _instrumentRepository.GetRiskRulesAsync(currentInstrumentId, cancellationToken);

        var scoringResult = _scoringEvaluator.Evaluate(expectedQuestionIds, answers, scoringRules);
        var riskResult = _riskEvaluator.Evaluate(answers, riskRules);

        var transitions = await _flowTransitionRepository.GetByFromInstrumentIdAsync(currentInstrumentId, cancellationToken);
        var decision = _flowEngine.Decide(riskResult, scoringResult, answers, transitions);

        var nextAction = decision.Outcome switch
        {
            FlowOutcome.Emergency => "emergency",
            FlowOutcome.Completed => "completed",
            FlowOutcome.ContinueToNextInstrument => "continue",
            _ => throw new InvalidOperationException($"Unhandled flow outcome: {decision.Outcome}")
        };

        await _resultRepository.AddAsync(
            new Result(0, sessionId, currentInstrumentId, scoringResult.TotalScore, scoringResult.Level, nextAction),
            cancellationToken);

        var newState = decision.Outcome switch
        {
            FlowOutcome.Emergency => SessionState.Emergency(),
            FlowOutcome.Completed => SessionState.Completed(),
            FlowOutcome.ContinueToNextInstrument => SessionState.AtInstrument(decision.NextInstrumentId!.Value),
            _ => throw new InvalidOperationException($"Unhandled flow outcome: {decision.Outcome}")
        };

        session.AdvanceTo(newState);
        await _sessionRepository.UpdateAsync(session, cancellationToken);
    }

    private async Task<IReadOnlyList<Answer>?> TryBuildCompletedInstrumentAnswersAsync(
        SessionId sessionId,
        Instrument instrument,
        CancellationToken cancellationToken)
    {
        var responses = await _responseRepository.GetBySessionIdAsync(sessionId, cancellationToken);
        var instrumentQuestionIds = instrument.Questions.Select(q => q.Id).ToHashSet();

        // Last answer wins per question, so a resubmitted answer doesn't get
        // double-counted into the total score.
        var latestResponsePerQuestion = responses
            .Where(r => instrumentQuestionIds.Contains(r.QuestionId))
            .GroupBy(r => r.QuestionId)
            .Select(g => g.OrderByDescending(r => r.AnsweredAt).First())
            .ToList();

        var isComplete = instrumentQuestionIds.All(id => latestResponsePerQuestion.Any(r => r.QuestionId == id));
        if (!isComplete)
        {
            return null;
        }

        var choicesByQuestion = instrument.Questions.ToDictionary(q => q.Id, q => q.Choices);

        return latestResponsePerQuestion
            .Select(r => new Answer(r.QuestionId, r.ChoiceId, choicesByQuestion[r.QuestionId].First(c => c.Id == r.ChoiceId).Score))
            .ToList();
    }
}
