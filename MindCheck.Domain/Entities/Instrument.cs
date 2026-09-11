using MindCheck.Domain.ValueObjects;

namespace MindCheck.Domain.Entities;

public sealed class Instrument
{
    private readonly List<Question> _questions;

    public InstrumentId Id { get; }
    public string Code { get; private set; }
    public string Name { get; private set; }
    public string Version { get; private set; }
    public string Source { get; private set; }
    public bool IsActive { get; private set; }
    public IReadOnlyList<Question> Questions => _questions;

    public Instrument(
        InstrumentId id,
        string code,
        string name,
        string version,
        string source,
        bool isActive,
        IReadOnlyList<Question> questions)
        : this(id, code, name, version, source, isActive)
    {
        _questions = questions is List<Question> list ? list : questions.ToList();
    }

    // EF Core materializes via this constructor (no navigation parameter) and
    // then populates _questions itself through the field-access navigation.
    private Instrument(InstrumentId id, string code, string name, string version, string source, bool isActive)
    {
        Id = id;
        Code = code;
        Name = name;
        Version = version;
        Source = source;
        IsActive = isActive;
        _questions = new List<Question>();
    }

    public void ChangeDetails(string code, string name, string version, string source, bool isActive)
    {
        Code = code;
        Name = name;
        Version = version;
        Source = source;
        IsActive = isActive;
    }

    public void AddQuestion(Question question)
    {
        _questions.Add(question);
    }

    public void RemoveQuestion(QuestionId id)
    {
        _questions.RemoveAll(q => q.Id == id);
    }
}
