using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MindCheck.Domain.ValueObjects;

namespace MindCheck.Infrastructure.Conversions;

// Domain models session progress as a small closed type (SessionState); the
// single "current_state" text column is an Infrastructure serialization
// concern, not something Domain should know how to stringify.
internal sealed class SessionStateValueConverter : ValueConverter<SessionState, string>
{
    public SessionStateValueConverter()
        : base(state => Serialize(state), value => Deserialize(value))
    {
    }

    private static string Serialize(SessionState state) => state.Status switch
    {
        SessionStatus.InProgress => $"InProgress:{state.CurrentInstrumentId!.Value.Value}",
        SessionStatus.Completed => "Completed",
        SessionStatus.Emergency => "Emergency",
        _ => throw new InvalidOperationException($"Unhandled session status: {state.Status}")
    };

    private static SessionState Deserialize(string value)
    {
        if (value == "Completed")
        {
            return SessionState.Completed();
        }

        if (value == "Emergency")
        {
            return SessionState.Emergency();
        }

        var parts = value.Split(':');
        if (parts.Length == 2 && parts[0] == "InProgress" && int.TryParse(parts[1], out var instrumentId))
        {
            return SessionState.AtInstrument(new InstrumentId(instrumentId));
        }

        throw new InvalidOperationException($"Cannot parse persisted session state '{value}'.");
    }
}
