namespace MindCheck.Application.Exceptions;

public sealed class DuplicateInstrumentCodeException : Exception
{
    public string Code { get; }

    public DuplicateInstrumentCodeException(string code)
        : base($"An instrument with code '{code}' already exists.")
    {
        Code = code;
    }
}
