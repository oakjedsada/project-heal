namespace MindCheck.Domain.ValueObjects;

// Reference type (not a struct) so Infrastructure can map it as an EF Core
// owned type; the record gives it structural equality regardless.
public sealed record RiskCondition
{
    public RiskOperator Operator { get; }
    public int Threshold { get; }

    public RiskCondition(RiskOperator @operator, int threshold)
    {
        Operator = @operator;
        Threshold = threshold;
    }

    public bool IsSatisfiedBy(int value) => Operator switch
    {
        RiskOperator.GreaterThan => value > Threshold,
        RiskOperator.GreaterThanOrEqual => value >= Threshold,
        RiskOperator.LessThan => value < Threshold,
        RiskOperator.LessThanOrEqual => value <= Threshold,
        RiskOperator.Equal => value == Threshold,
        RiskOperator.NotEqual => value != Threshold,
        _ => throw new ArgumentOutOfRangeException(nameof(Operator), Operator, "Unsupported risk operator.")
    };
}
