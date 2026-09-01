using MindCheck.Domain.Entities;
using MindCheck.Domain.ValueObjects;

namespace MindCheck.Infrastructure.Seed;

/// <summary>
/// Placeholder assessment content for ST-5 / 2Q / 9Q / 8Q. All scoring cut-offs
/// and risk thresholds here are clearly-labeled dummy numbers (Source =
/// "PLACEHOLDER-DO-NOT-USE-CLINICALLY") to exercise the engine end-to-end.
/// Real question text and real cut-offs are supplied by the project owner later
/// as pure data changes — no code here should need to change when that happens.
/// </summary>
internal static class SeedData
{
    private const string PlaceholderSource = "PLACEHOLDER-DO-NOT-USE-CLINICALLY";

    public static readonly Instrument[] Instruments =
    {
        new(new InstrumentId(1), "ST5", "ST-5 (placeholder)", "1.0", PlaceholderSource, true, Array.Empty<Question>()),
        new(new InstrumentId(2), "2Q", "2Q (placeholder)", "1.0", PlaceholderSource, true, Array.Empty<Question>()),
        new(new InstrumentId(3), "9Q", "9Q (placeholder)", "1.0", PlaceholderSource, true, Array.Empty<Question>()),
        new(new InstrumentId(4), "8Q", "8Q (placeholder)", "1.0", PlaceholderSource, true, Array.Empty<Question>()),
    };

    public static readonly Question[] Questions = BuildQuestions();

    public static readonly Choice[] Choices = BuildChoices();

    public static readonly ScoringRule[] ScoringRules =
    {
        new(1, new InstrumentId(1), new ScoreRange(0, 4), "Low", "[PLACEHOLDER] ระดับความเครียดต่ำ", "[PLACEHOLDER] คำแนะนำระดับต่ำ"),
        new(2, new InstrumentId(1), new ScoreRange(5, 9), "Medium", "[PLACEHOLDER] ระดับความเครียดปานกลาง", "[PLACEHOLDER] คำแนะนำระดับปานกลาง"),
        new(3, new InstrumentId(1), new ScoreRange(10, 15), "High", "[PLACEHOLDER] ระดับความเครียดสูง", "[PLACEHOLDER] คำแนะนำระดับสูง"),

        new(4, new InstrumentId(2), new ScoreRange(0, 0), "Negative", "[PLACEHOLDER] ไม่พบสัญญาณ", "[PLACEHOLDER] คำแนะนำ"),
        new(5, new InstrumentId(2), new ScoreRange(1, 2), "Positive", "[PLACEHOLDER] พบสัญญาณ ควรประเมินต่อ", "[PLACEHOLDER] คำแนะนำ"),

        new(6, new InstrumentId(3), new ScoreRange(0, 9), "Low", "[PLACEHOLDER] ระดับต่ำ", "[PLACEHOLDER] คำแนะนำ"),
        new(7, new InstrumentId(3), new ScoreRange(10, 19), "Medium", "[PLACEHOLDER] ระดับปานกลาง", "[PLACEHOLDER] คำแนะนำ"),
        new(8, new InstrumentId(3), new ScoreRange(20, 27), "High", "[PLACEHOLDER] ระดับสูง", "[PLACEHOLDER] คำแนะนำ"),

        new(9, new InstrumentId(4), new ScoreRange(0, 7), "Low", "[PLACEHOLDER] ระดับต่ำ", "[PLACEHOLDER] คำแนะนำ"),
        new(10, new InstrumentId(4), new ScoreRange(8, 15), "Medium", "[PLACEHOLDER] ระดับปานกลาง", "[PLACEHOLDER] คำแนะนำ"),
        new(11, new InstrumentId(4), new ScoreRange(16, 24), "High", "[PLACEHOLDER] ระดับสูง", "[PLACEHOLDER] คำแนะนำ"),
    };

    // The only risk rule: 8Q's last item (id 24) at/above 1 point escalates to emergency.
    public static readonly RiskRule[] RiskRules =
    {
        new(1, new InstrumentId(4), new QuestionId(24), new RiskCondition(RiskOperator.GreaterThanOrEqual, 1), "emergency"),
    };

    public static readonly FlowTransition[] FlowTransitions =
    {
        // Session start (from = null) always begins at ST-5.
        new(1, null, FlowConditionType.Always, null, null, new InstrumentId(1)),
        // ST-5 always continues to 2Q.
        new(2, new InstrumentId(1), FlowConditionType.Always, null, null, new InstrumentId(2)),
        // 2Q "Positive" continues to 9Q; otherwise no row matches -> Completed.
        new(3, new InstrumentId(2), FlowConditionType.ScoreLevelEquals, null, "Positive", new InstrumentId(3)),
        // 9Q's last item (id 16, order 9) at/above 1 point continues to 8Q.
        new(4, new InstrumentId(3), FlowConditionType.QuestionScoreAtLeast, new QuestionId(16), "1", new InstrumentId(4)),
    };

    private static Question[] BuildQuestions()
    {
        var list = new List<Question>();

        for (var i = 1; i <= 5; i++)
        {
            list.Add(new Question(new QuestionId(i), new InstrumentId(1), i, $"[PLACEHOLDER] ST-5 ข้อที่ {i}", QuestionType.SingleChoice, Array.Empty<Choice>()));
        }

        for (var i = 0; i < 2; i++)
        {
            list.Add(new Question(new QuestionId(6 + i), new InstrumentId(2), i + 1, $"[PLACEHOLDER] 2Q ข้อที่ {i + 1}", QuestionType.SingleChoice, Array.Empty<Choice>()));
        }

        for (var i = 0; i < 9; i++)
        {
            list.Add(new Question(new QuestionId(8 + i), new InstrumentId(3), i + 1, $"[PLACEHOLDER] 9Q ข้อที่ {i + 1}", QuestionType.SingleChoice, Array.Empty<Choice>()));
        }

        for (var i = 0; i < 8; i++)
        {
            list.Add(new Question(new QuestionId(17 + i), new InstrumentId(4), i + 1, $"[PLACEHOLDER] 8Q ข้อที่ {i + 1}", QuestionType.SingleChoice, Array.Empty<Choice>()));
        }

        return list.ToArray();
    }

    private static Choice[] BuildChoices()
    {
        var list = new List<Choice>();
        string[] fourPoint = { "ไม่เลย", "เล็กน้อย", "ปานกลาง", "มาก" };
        string[] twoPoint = { "ไม่มี", "มี" };

        // ST-5 (1-5), 9Q (8-16) and 8Q (17-24) share a 0-3 four-point scale.
        foreach (var questionId in Enumerable.Range(1, 5).Concat(Enumerable.Range(8, 9)).Concat(Enumerable.Range(17, 8)))
        {
            for (var score = 0; score < fourPoint.Length; score++)
            {
                list.Add(new Choice(new ChoiceId(questionId * 10 + score + 1), new QuestionId(questionId), fourPoint[score], score, score + 1));
            }
        }

        // 2Q (6-7) is a yes/no 0-1 scale.
        foreach (var questionId in Enumerable.Range(6, 2))
        {
            for (var score = 0; score < twoPoint.Length; score++)
            {
                list.Add(new Choice(new ChoiceId(questionId * 10 + score + 1), new QuestionId(questionId), twoPoint[score], score, score + 1));
            }
        }

        return list.ToArray();
    }
}
