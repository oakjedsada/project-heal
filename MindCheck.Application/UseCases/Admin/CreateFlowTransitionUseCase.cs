using MindCheck.Application.Abstractions;
using MindCheck.Application.Dtos.Admin;
using MindCheck.Application.Exceptions;
using MindCheck.Domain.Entities;
using MindCheck.Domain.ValueObjects;

namespace MindCheck.Application.UseCases.Admin;

public sealed class CreateFlowTransitionUseCase
{
    private readonly IFlowTransitionRepository _flowTransitionRepository;
    private readonly IInstrumentRepository _instrumentRepository;

    public CreateFlowTransitionUseCase(
        IFlowTransitionRepository flowTransitionRepository,
        IInstrumentRepository instrumentRepository)
    {
        _flowTransitionRepository = flowTransitionRepository;
        _instrumentRepository = instrumentRepository;
    }

    public async Task<FlowTransitionDto> ExecuteAsync(CreateFlowTransitionRequest request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<FlowConditionType>(request.ConditionType, ignoreCase: true, out var conditionType))
        {
            throw new InvalidAdminRequestException($"Unknown condition type '{request.ConditionType}'.");
        }

        var toInstrumentId = new InstrumentId(request.ToInstrumentId);
        var toInstrument = await _instrumentRepository.GetByIdAsync(toInstrumentId, cancellationToken)
            ?? throw new InstrumentNotFoundException(toInstrumentId);

        Instrument? fromInstrument = null;
        if (request.FromInstrumentId.HasValue)
        {
            var fromInstrumentId = new InstrumentId(request.FromInstrumentId.Value);
            fromInstrument = await _instrumentRepository.GetByIdAsync(fromInstrumentId, cancellationToken)
                ?? throw new InstrumentNotFoundException(fromInstrumentId);
        }

        QuestionId? questionId = null;

        switch (conditionType)
        {
            case FlowConditionType.Always:
                break;
            case FlowConditionType.ScoreLevelEquals:
                if (string.IsNullOrWhiteSpace(request.ConditionValue))
                {
                    throw new InvalidAdminRequestException("ScoreLevelEquals requires a condition value (the level name).");
                }
                break;
            case FlowConditionType.QuestionScoreAtLeast:
                if (request.QuestionId is null)
                {
                    throw new InvalidAdminRequestException("QuestionScoreAtLeast requires a question id.");
                }
                if (fromInstrument is null)
                {
                    throw new InvalidAdminRequestException("QuestionScoreAtLeast requires a from-instrument to validate the question against.");
                }
                if (!int.TryParse(request.ConditionValue, out _))
                {
                    throw new InvalidAdminRequestException("QuestionScoreAtLeast requires a numeric condition value (the threshold).");
                }
                questionId = new QuestionId(request.QuestionId.Value);
                if (!fromInstrument.Questions.Any(q => q.Id == questionId))
                {
                    throw new InvalidAdminRequestException(
                        $"Question {request.QuestionId} does not belong to instrument {fromInstrument.Code}.");
                }
                break;
        }

        var transition = new FlowTransition(
            0,
            fromInstrument?.Id,
            conditionType,
            questionId,
            request.ConditionValue,
            toInstrument.Id);

        var saved = await _flowTransitionRepository.AddAsync(transition, cancellationToken);

        return new FlowTransitionDto(
            saved.Id,
            saved.FromInstrumentId?.Value,
            fromInstrument?.Code,
            saved.ConditionType.ToString(),
            saved.QuestionId?.Value,
            saved.ConditionValue,
            saved.ToInstrumentId.Value,
            toInstrument.Code);
    }
}
