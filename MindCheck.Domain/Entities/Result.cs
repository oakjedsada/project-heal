using MindCheck.Domain.ValueObjects;

namespace MindCheck.Domain.Entities;

public sealed class Result
{
    public int Id { get; }
    public SessionId SessionId { get; }
    public InstrumentId InstrumentId { get; }
    public int TotalScore { get; }
    public string Level { get; }
    public string NextAction { get; }

    public Result(int id, SessionId sessionId, InstrumentId instrumentId, int totalScore, string level, string nextAction)
    {
        Id = id;
        SessionId = sessionId;
        InstrumentId = instrumentId;
        TotalScore = totalScore;
        Level = level;
        NextAction = nextAction;
    }
}
