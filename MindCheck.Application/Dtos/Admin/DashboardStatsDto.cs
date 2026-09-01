namespace MindCheck.Application.Dtos.Admin;

public sealed record LevelBreakdownDto(string InstrumentCode, string Level, int Count);

public sealed record WeeklyTrendPointDto(DateOnly WeekStart, int SessionCount);

public sealed record DashboardStatsDto(
    IReadOnlyList<LevelBreakdownDto> LevelBreakdown,
    IReadOnlyList<WeeklyTrendPointDto> WeeklyTrend);
