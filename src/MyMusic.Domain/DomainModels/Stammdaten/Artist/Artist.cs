namespace MyMusic.Domain.DomainModels.Stammdaten.Artist;

public sealed class Artist
{
    public const int MinNameLength = 3;

    public const int MaxNameLength = 120;

    public const string NamePattern = @"^[\p{L}\p{N} \-&'./]+$";

    public int Id { get; private init; }

    public string Name { get; private init; }

    public Guid UserId { get; private init; }

    internal Artist(int id, string name, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Der Name des Artists darf nicht leer sein.", nameof(name));

        if (name.Length < MinNameLength)
            throw new ArgumentException(
                $"Der Name des Artists muss mindestens {MinNameLength} Zeichen lang sein.",
                nameof(name));

        if (name.Length > MaxNameLength)
            throw new ArgumentException(
                $"Der Name des Artists darf höchstens {MaxNameLength} Zeichen lang sein.",
                nameof(name));

        if (!Regex.IsMatch(name, NamePattern))
            throw new ArgumentException(
                "Der Name des Artists darf nur Buchstaben, Zahlen, Leerzeichen sowie - & ' . / enthalten.",
                nameof(name));

        Id = id;

        Name = name;

        UserId = userId;
    }

    public static Artist Create(string name, Guid userId)
    {
        return new Artist(0, name, userId);
    }

    public Artist Update(string name)
    {
        return new Artist(Id, name, UserId);
    }
}
