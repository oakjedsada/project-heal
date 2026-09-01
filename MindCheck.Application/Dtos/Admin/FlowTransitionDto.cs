namespace MindCheck.Application.Dtos.Admin;

public sealed record FlowTransitionDto(
    int Id,
    int? FromInstrumentId,
    string? FromInstrumentCode,
    string ConditionType,
    int? QuestionId,
    string? ConditionValue,
    int ToInstrumentId,
    string ToInstrumentCode);

public sealed record CreateFlowTransitionRequest(
    int? FromInstrumentId,
    string ConditionType,
    int? QuestionId,
    string? ConditionValue,
    int ToInstrumentId);
