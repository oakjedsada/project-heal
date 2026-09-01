namespace MindCheck.Application.Dtos;

public sealed record InstrumentResultDto(
    string InstrumentCode,
    int TotalScore,
    string Level,
    string Interpretation,
    string Advice);
