namespace MindCheck.Application.Dtos.Admin;

public sealed record InstrumentSummaryDto(int InstrumentId, string Code, string Name, bool IsActiveStart);
