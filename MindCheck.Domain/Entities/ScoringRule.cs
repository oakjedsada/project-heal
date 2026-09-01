using MindCheck.Domain.ValueObjects;

namespace MindCheck.Domain.Entities;

public sealed class ScoringRule
{
    private ScoreRange _range = null!;

    public int Id { get; }
    public InstrumentId InstrumentId { get; }
    public ScoreRange Range => _range;
    public string Level { get; }
    public string Interpretation { get; }
    public string Advice { get; }

    public ScoringRule(
        int id,
        InstrumentId instrumentId,
        ScoreRange range,
        string level,
        string interpretation,
        string advice)
        : this(id, instrumentId, level, interpretation, advice)
    {
        _range = range;
    }

    // EF Core materializes via this constructor (owned types can't be
    // constructor-bound) and then populates _range through field access.
    private ScoringRule(int id, InstrumentId instrumentId, string level, string interpretation, string advice)
    {
        Id = id;
        InstrumentId = instrumentId;
        Level = level;
        Interpretation = interpretation;
        Advice = advice;
    }
}
