namespace MindCheck.Application.Dtos.Admin;

public sealed record InstrumentDetailQuestionDto(int QuestionId, string Text, int OrderNo);

public sealed record InstrumentDetailDto(
    int InstrumentId,
    string Code,
    string Name,
    IReadOnlyList<InstrumentDetailQuestionDto> Questions);
