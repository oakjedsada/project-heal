namespace MindCheck.Application.Dtos;

// Deliberately excludes Score: the frontend must never see or compute scoring.
public sealed record ChoiceDto(int ChoiceId, string Label, int OrderNo);
