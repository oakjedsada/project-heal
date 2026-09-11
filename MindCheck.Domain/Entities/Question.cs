using MindCheck.Domain.ValueObjects;

namespace MindCheck.Domain.Entities;

public sealed class Question
{
    private readonly List<Choice> _choices;

    public QuestionId Id { get; }
    public InstrumentId InstrumentId { get; }
    public int OrderNo { get; private set; }
    public string Text { get; private set; }
    public QuestionType QuestionType { get; }

    // Backing field name follows EF Core's convention so it can bind the
    // navigation without a public setter, keeping this collection read-only
    // from every consumer outside Infrastructure.
    public IReadOnlyList<Choice> Choices => _choices;

    public Question(
        QuestionId id,
        InstrumentId instrumentId,
        int orderNo,
        string text,
        QuestionType questionType,
        IReadOnlyList<Choice> choices)
        : this(id, instrumentId, orderNo, text, questionType)
    {
        _choices = choices is List<Choice> list ? list : choices.ToList();
    }

    // EF Core materializes via this constructor (no navigation parameter) and
    // then populates _choices itself through the field-access navigation.
    private Question(QuestionId id, InstrumentId instrumentId, int orderNo, string text, QuestionType questionType)
    {
        Id = id;
        InstrumentId = instrumentId;
        OrderNo = orderNo;
        Text = text;
        QuestionType = questionType;
        _choices = new List<Choice>();
    }

    public void ChangeDetails(string text, int orderNo)
    {
        Text = text;
        OrderNo = orderNo;
    }

    public void AddChoice(Choice choice)
    {
        _choices.Add(choice);
    }

    public void RemoveChoice(ChoiceId id)
    {
        _choices.RemoveAll(c => c.Id == id);
    }
}
