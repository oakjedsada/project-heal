using MindCheck.Application.Abstractions;
using MindCheck.Application.Dtos.Admin;

namespace MindCheck.Application.UseCases.Admin;

public sealed class GetDashboardStatsUseCase
{
    private readonly IResultRepository _resultRepository;
    private readonly ISessionRepository _sessionRepository;
    private readonly IInstrumentRepository _instrumentRepository;

    public GetDashboardStatsUseCase(
        IResultRepository resultRepository,
        ISessionRepository sessionRepository,
        IInstrumentRepository instrumentRepository)
    {
        _resultRepository = resultRepository;
        _sessionRepository = sessionRepository;
        _instrumentRepository = instrumentRepository;
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
        var weeklyTrend = startedAtTimes
            .GroupBy(t => StartOfIsoWeek(DateOnly.FromDateTime(t.UtcDateTime)))
            .Select(g => new WeeklyTrendPointDto(g.Key, g.Count()))
            .OrderBy(p => p.WeekStart)
            .ToList();

        return new DashboardStatsDto(levelBreakdown, weeklyTrend);
    }

    private static DateOnly StartOfIsoWeek(DateOnly date)
    {
        var diff = ((int)date.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        return date.AddDays(-diff);
    }
}
