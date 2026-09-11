namespace MindCheck.Application.Dtos.Admin;

public sealed record LevelBreakdownDto(string InstrumentCode, string Level, int Count);

public sealed record DailyTrendPointDto(DateOnly Date, int SessionCount);

public sealed record DashboardStatsDto(
    IReadOnlyList<LevelBreakdownDto> LevelBreakdown,
    IReadOnlyList<DailyTrendPointDto> DailyTrend);
