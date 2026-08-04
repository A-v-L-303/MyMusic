namespace MyMusic.Domain.DomainModels.Stammdaten.Genre;

public sealed class Genre
{
    public const int MinNameLength = 3;

    public const int MaxNameLength = 50;

    public const string NamePattern = @"^[\p{L}\p{N} \-&']+$";

    public int Id { get; private init; }

    public string Name { get; private init; }

    public Guid UserId { get; private init; }

    internal Genre(int id, string name, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Der Name des Genres darf nicht leer sein.", nameof(name));

        if (name.Length < MinNameLength)
            throw new ArgumentException(
                $"Der Name des Genres muss mindestens {MinNameLength} Zeichen lang sein.",
                nameof(name));

        if (name.Length > MaxNameLength)
            throw new ArgumentException(
                $"Der Name des Genres darf höchstens {MaxNameLength} Zeichen lang sein.",
                nameof(name));

        if (!Regex.IsMatch(name, NamePattern))
            throw new ArgumentException(
                "Der Name des Genres darf nur Buchstaben, Zahlen, Leerzeichen sowie - & ' enthalten.",
                nameof(name));

        Id = id;

        Name = name;

        UserId = userId;
    }

    public static Genre Create(string name, Guid userId)
    {
        return new Genre(0, name, userId);
    }

    public Genre Update(string name)
    {
        return new Genre(Id, name, UserId);
    }
}
