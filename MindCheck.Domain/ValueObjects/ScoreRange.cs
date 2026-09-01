namespace MindCheck.Domain.ValueObjects;

// Reference type (not a struct) so Infrastructure can map it as an EF Core
// owned type; the record gives it structural equality regardless.
public sealed record ScoreRange
{
    public int Min { get; }
    public int Max { get; }

    public ScoreRange(int min, int max)
    {
        if (min > max)
        {
            throw new ArgumentException("Min must not exceed Max.", nameof(min));
        }

        Min = min;
        Max = max;
    }

    public bool Contains(int score) => score >= Min && score <= Max;
}
