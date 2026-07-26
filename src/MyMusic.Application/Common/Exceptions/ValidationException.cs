namespace MyMusic.Application.Common.Exceptions;

public sealed class ValidationException(IReadOnlyDictionary<string, string[]> errors)
    : Exception("Die Eingabe enthält ungültige Werte.")
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = errors;
}
