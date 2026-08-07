namespace MyMusic.Domain.DomainModels.Stammdaten.Label;

public sealed class Label
{
    public const int MinNameLength = 1;

    public const int MaxNameLength = 60;

    public const int MaxInformationLength = 255;

    public const string NamePattern = @"^[\p{L}\p{N} \-&'./]+$";

    public int Id { get; private init; }

    public string Name { get; private init; }

    public int CountryId { get; private init; }

    public string? Information { get; private init; }

    public Guid UserId { get; private init; }

    internal Label(int id, string name, int countryId, string? information, Guid userId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Der Name des Labels darf nicht leer sein.", nameof(name));

        if (name.Length < MinNameLength)
            throw new ArgumentException(
                $"Der Name des Labels muss mindestens {MinNameLength} Zeichen lang sein.",
                nameof(name));

        if (name.Length > MaxNameLength)
            throw new ArgumentException(
                $"Der Name des Labels darf höchstens {MaxNameLength} Zeichen lang sein.",
                nameof(name));

        if (!Regex.IsMatch(name, NamePattern))
            throw new ArgumentException(
                "Der Name des Labels darf nur Buchstaben, Zahlen, Leerzeichen sowie - & ' . / enthalten.",
                nameof(name));

        if (countryId <= 0)
            throw new ArgumentException("Das Herkunftsland ist erforderlich.", nameof(countryId));

        if (!string.IsNullOrEmpty(information) && information.Length > MaxInformationLength)
            throw new ArgumentException(
                $"Das Feld 'information' darf höchstens {MaxInformationLength} Zeichen lang sein.",
                nameof(information));

        Id = id;

        Name = name;

        CountryId = countryId;

        Information = information;

        UserId = userId;
    }

    public static Label Create(string name, int countryId, string? information, Guid userId)
    {
        return new Label(0, name, countryId, information, userId);
    }

    public Label Update(string name, int countryId, string? information)
    {
        return new Label(Id, name, countryId, information, UserId);
    }
}
