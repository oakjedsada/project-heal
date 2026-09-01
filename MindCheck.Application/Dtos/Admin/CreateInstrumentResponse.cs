namespace MindCheck.Application.Dtos.Admin;

public sealed record CreatedChoiceDto(int ChoiceId, string Label, int Score, int OrderNo);

public sealed record CreatedQuestionDto(int QuestionId, string Text, int OrderNo, IReadOnlyList<CreatedChoiceDto> Choices);

public sealed record CreateInstrumentResponse(
    int InstrumentId,
    string Code,
    IReadOnlyList<CreatedQuestionDto> Questions);
