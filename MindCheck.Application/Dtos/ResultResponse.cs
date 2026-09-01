namespace MindCheck.Application.Dtos;

public sealed record ResultResponse(
    bool IsComplete,
    string NextAction,
    IReadOnlyList<InstrumentResultDto> Results,
    HelpResourceDto[]? HelpResources);
