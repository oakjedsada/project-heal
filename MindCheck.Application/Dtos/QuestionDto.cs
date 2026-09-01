namespace MindCheck.Application.Dtos;

public sealed record QuestionDto(int QuestionId, string Text, IReadOnlyList<ChoiceDto> Choices);
