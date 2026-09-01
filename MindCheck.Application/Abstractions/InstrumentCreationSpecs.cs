using MindCheck.Domain.ValueObjects;

namespace MindCheck.Application.Abstractions;

// Plain specs (not Domain objects) for the parts of a new instrument that
// can't be fully constructed until the instrument + its questions have real,
// database-generated ids — which only exist after the first save. Building
// the actual ScoringRule/RiskRule Domain objects from these is Infrastructure's
// job (it already owns the two-phase save + transaction).
public sealed record ScoringRuleSpec(int Min, int Max, string Level, string Interpretation, string Advice);

public sealed record RiskRuleSpec(int QuestionOrderNo, RiskOperator Operator, int Threshold, string Action);
