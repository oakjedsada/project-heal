using MindCheck.Application.Abstractions;
using MindCheck.Application.Dtos.Admin;

namespace MindCheck.Application.UseCases.Admin;

public sealed class ListFlowTransitionsUseCase
{
    private readonly IFlowTransitionRepository _flowTransitionRepository;
    private readonly IInstrumentRepository _instrumentRepository;

    public ListFlowTransitionsUseCase(
        IFlowTransitionRepository flowTransitionRepository,
        IInstrumentRepository instrumentRepository)
    {
        _flowTransitionRepository = flowTransitionRepository;
        _instrumentRepository = instrumentRepository;
    }

    public async Task<IReadOnlyList<FlowTransitionDto>> ExecuteAsync(CancellationToken cancellationToken)
    {
        var transitions = await _flowTransitionRepository.GetAllAsync(cancellationToken);
        var instruments = await _instrumentRepository.GetAllAsync(cancellationToken);
        var codeById = instruments.ToDictionary(i => i.Id, i => i.Code);

        return transitions
            .OrderBy(t => t.Id)
            .Select(t => new FlowTransitionDto(
                t.Id,
                t.FromInstrumentId?.Value,
                t.FromInstrumentId.HasValue ? codeById.GetValueOrDefault(t.FromInstrumentId.Value) : null,
                t.ConditionType.ToString(),
                t.QuestionId?.Value,
                t.ConditionValue,
                t.ToInstrumentId.Value,
                codeById.GetValueOrDefault(t.ToInstrumentId) ?? "?"))
            .ToList();
    }
}
