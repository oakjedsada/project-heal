using MindCheck.Domain.ValueObjects;

namespace MindCheck.Domain.Entities;

public sealed class RiskRule
{
    private RiskCondition _condition = null!;

    public int Id { get; }
    public InstrumentId InstrumentId { get; }
    public QuestionId QuestionId { get; }
    public RiskCondition Condition => _condition;
    public string Action { get; }

    public RiskRule(
        int id,
        InstrumentId instrumentId,
        QuestionId questionId,
        RiskCondition condition,
        string action)
        : this(id, instrumentId, questionId, action)
    {
        _condition = condition;
    }

    // EF Core materializes via this constructor (owned types can't be
    // constructor-bound) and then populates _condition through field access.
    private RiskRule(int id, InstrumentId instrumentId, QuestionId questionId, string action)
    {
        Id = id;
        InstrumentId = instrumentId;
        QuestionId = questionId;
        Action = action;
    }
}
