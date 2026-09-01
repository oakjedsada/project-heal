namespace MindCheck.Domain.ValueObjects;

public readonly record struct InstrumentId(int Value);

public readonly record struct QuestionId(int Value);

public readonly record struct ChoiceId(int Value);

public readonly record struct SessionId(Guid Value);
