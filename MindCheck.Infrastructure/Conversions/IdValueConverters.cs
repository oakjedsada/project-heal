using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MindCheck.Domain.ValueObjects;

namespace MindCheck.Infrastructure.Conversions;

internal static class IdValueConverters
{
    public static ValueConverter<InstrumentId, int> InstrumentId { get; } =
        new(id => id.Value, value => new InstrumentId(value));

    public static ValueConverter<InstrumentId?, int?> NullableInstrumentId { get; } = new(
        id => id.HasValue ? id.Value.Value : null,
        value => value.HasValue ? new InstrumentId(value.Value) : null);

    public static ValueConverter<QuestionId, int> QuestionId { get; } =
        new(id => id.Value, value => new QuestionId(value));

    public static ValueConverter<QuestionId?, int?> NullableQuestionId { get; } = new(
        id => id.HasValue ? id.Value.Value : null,
        value => value.HasValue ? new QuestionId(value.Value) : null);

    public static ValueConverter<ChoiceId, int> ChoiceId { get; } =
        new(id => id.Value, value => new ChoiceId(value));

    public static ValueConverter<SessionId, Guid> SessionId { get; } =
        new(id => id.Value, value => new SessionId(value));
}
