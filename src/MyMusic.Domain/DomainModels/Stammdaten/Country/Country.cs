namespace MyMusic.Domain.DomainModels.Stammdaten.Country;

public sealed class Country
{
    public const int MaxNameLength = 50;

    public const int MaxCodeLength = 3;

    public int Id { get; private init; }

    public string Name { get; private init; }

    public string Code { get; private init; }

    internal Country(int id, string name, string code)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Der Name des Landes darf nicht leer sein.", nameof(name));

        if (name.Length > MaxNameLength)
            throw new ArgumentException(
                $"Der Name des Landes darf höchstens {MaxNameLength} Zeichen lang sein.",
                nameof(name));

        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Der Ländercode darf nicht leer sein.", nameof(code));

        if (code.Length > MaxCodeLength)
            throw new ArgumentException(
                $"Der Ländercode darf höchstens {MaxCodeLength} Zeichen lang sein.",
                nameof(code));

        Id = id;

        Name = name;

        Code = code;
    }

    public static Country Create(string name, string code)
    {
        return new Country(0, name, code);
    }
}
