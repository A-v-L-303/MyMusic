namespace MyMusic.Domain.DomainModels.Sammlung.Record;

public sealed class Record
{
    public const int MinAlbumNameLength = 1;

    public const int MaxAlbumNameLength = 150;

    public const int MaxInformationLength = 255;

    public const int MinReleaseYear = 1860;

    public const string AlbumNamePattern = @"^[\p{L}\p{N} \-&'./()]+$";

    public int Id { get; private init; }

    public int LabelId { get; private init; }

    public int? ArtistId { get; private init; }

    public byte[]? AlbumCover { get; private init; }

    public RecordFormat Format { get; private init; }

    public string AlbumName { get; private init; }

    public int ReleaseYear { get; private init; }

    public RecordCondition Condition { get; private init; }

    public string? Information { get; private init; }

    public Guid UserId { get; private init; }

    internal Record(
        int id,
        int labelId,
        int? artistId,
        byte[]? albumCover,
        RecordFormat format,
        string albumName,
        int releaseYear,
        RecordCondition condition,
        string? information,
        Guid userId)
    {
        if (labelId <= 0)
            throw new ArgumentException("Das Label ist erforderlich.", nameof(labelId));

        if (string.IsNullOrWhiteSpace(albumName))
            throw new ArgumentException("Der Albumname darf nicht leer sein.", nameof(albumName));

        if (albumName.Length < MinAlbumNameLength)
            throw new ArgumentException(
                $"Der Albumname muss mindestens {MinAlbumNameLength} Zeichen lang sein.",
                nameof(albumName));

        if (albumName.Length > MaxAlbumNameLength)
            throw new ArgumentException(
                $"Der Albumname darf höchstens {MaxAlbumNameLength} Zeichen lang sein.",
                nameof(albumName));

        if (!Regex.IsMatch(albumName, AlbumNamePattern))
            throw new ArgumentException(
                "Der Albumname darf nur Buchstaben, Zahlen, Leerzeichen sowie - & ' . / ( ) enthalten.",
                nameof(albumName));

        var currentYear = DateTime.UtcNow.Year;

        if (releaseYear < MinReleaseYear || releaseYear > currentYear)
            throw new ArgumentException(
                $"Das Erscheinungsjahr muss zwischen {MinReleaseYear} und {currentYear} liegen.",
                nameof(releaseYear));

        if (!string.IsNullOrEmpty(information) && information.Length > MaxInformationLength)
            throw new ArgumentException(
                $"Das Feld 'information' darf höchstens {MaxInformationLength} Zeichen lang sein.",
                nameof(information));

        Id = id;

        LabelId = labelId;

        ArtistId = artistId;

        AlbumCover = albumCover;

        Format = format;

        AlbumName = albumName;

        ReleaseYear = releaseYear;

        Condition = condition;

        Information = information;

        UserId = userId;
    }

    public static Record Create(
        int labelId,
        int? artistId,
        RecordFormat format,
        string albumName,
        int releaseYear,
        RecordCondition condition,
        string? information,
        Guid userId)
    {
        return new Record(0, labelId, artistId, null, format, albumName, releaseYear, condition, information, userId);
    }

    public Record Update(
        int labelId,
        int? artistId,
        RecordFormat format,
        string albumName,
        int releaseYear,
        RecordCondition condition,
        string? information)
    {
        return new Record(
            Id,
            labelId,
            artistId,
            AlbumCover,
            format,
            albumName,
            releaseYear,
            condition,
            information,
            UserId);
    }
}
