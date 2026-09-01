namespace MindCheck.Application.Dtos;

public sealed record NextStepResponse(bool IsComplete, QuestionDto? Question);
