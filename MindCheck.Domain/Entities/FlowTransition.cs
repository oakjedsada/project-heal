using MindCheck.Domain.ValueObjects;

namespace MindCheck.Domain.Entities;

public sealed class FlowTransition
{
    public int Id { get; }

    /// <summary>Null represents the session-start transition (no instrument completed yet).</summary>
    public InstrumentId? FromInstrumentId { get; }

    public FlowConditionType ConditionType { get; }
    public QuestionId? QuestionId { get; }
    public string? ConditionValue { get; }
    public InstrumentId ToInstrumentId { get; }

    public FlowTransition(
        int id,
        InstrumentId? fromInstrumentId,
        FlowConditionType conditionType,
        QuestionId? questionId,
        string? conditionValue,
        InstrumentId toInstrumentId)
    {
        Id = id;
        FromInstrumentId = fromInstrumentId;
        ConditionType = conditionType;
        QuestionId = questionId;
        ConditionValue = conditionValue;
        ToInstrumentId = toInstrumentId;
    }
}
