using MindCheck.Application.Abstractions;
using MindCheck.Application.Dtos.Admin;

namespace MindCheck.Application.UseCases.Admin;

public sealed class GetDashboardStatsUseCase
{
    // A rolling window, not a calendar week — chosen so the chart always has
    // several points to draw a trend line, even right after a fresh deploy
    // when all activity so far falls inside a single ISO week (the previous
    // behavior collapsed to one point in that case, which a line chart can't
    // draw a line through at all).
    private const int TrendWindowDays = 7;

    private readonly IResultRepository _resultRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly IInstrumentRepository _instrumentRepository;
    private readonly TimeProvider _timeProvider;

    public GetDashboardStatsUseCase(
        IResultRepository resultRepository,
        ISessionRepository sessionRepository,
        IInstrumentRepository instrumentRepository,
        TimeProvider timeProvider)
    {
        _resultRepository = resultRepository;
        _sessionRepository = sessionRepository;
        _instrumentRepository = instrumentRepository;
        _timeProvider = timeProvider;
    }

    public async Task<DashboardStatsDto> ExecuteAsync(CancellationToken cancellationToken)
    {
        var levelCounts = await _resultRepository.GetLevelCountsAsync(cancellationToken);
        var instruments = await _instrumentRepository.GetAllAsync(cancellationToken);
        var codeById = instruments.ToDictionary(i => i.Id, i => i.Code);

        var levelBreakdown = levelCounts
            .Select(lc => new LevelBreakdownDto(codeById.GetValueOrDefault(lc.InstrumentId) ?? "?", lc.Level, lc.Count))
            .OrderBy(d => d.InstrumentCode)
            .ThenBy(d => d.Level)
            .ToList();

        var startedAtTimes = await _sessionRepository.GetAllStartedAtAsync(cancellationToken);
        var countsByDay = startedAtTimes
            .GroupBy(t => DateOnly.FromDateTime(t.UtcDateTime))
            .ToDictionary(g => g.Key, g => g.Count());

        var today = DateOnly.FromDateTime(_timeProvider.GetUtcNow().UtcDateTime);
        var dailyTrend = Enumerable.Range(0, TrendWindowDays)
            .Select(daysAgo => today.AddDays(daysAgo - (TrendWindowDays - 1)))
            .Select(day => new DailyTrendPointDto(day, countsByDay.GetValueOrDefault(day)))
            .ToList();

        return new DashboardStatsDto(levelBreakdown, dailyTrend);
    }
}
