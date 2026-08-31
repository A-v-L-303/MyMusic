namespace MyMusic.Domain.DomainModels.Sammlung.Record;

public sealed class Record
{
    public const int MinAlbumNameLength = 1;

    public const int MaxAlbumNameLength = 150;

    public const int MaxInformationLength = 255;

    public const int MinReleaseYear = 1860;

    public const string AlbumNamePattern = @"^[\p{L}\p{N} \-&'./()]+$";

    public const int MaxAlbumCoverSizeBytes = 5 * 1024 * 1024;

    private static readonly byte[] _jpegSignature = [0xFF, 0xD8, 0xFF];

    private static readonly byte[] _pngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

    public int Id { get; private init; }

    public int CollectionNumber { get; private init; }

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
        int collectionNumber,
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
        if (collectionNumber <= 0)
            throw new ArgumentException(
                "Die Sammlungsnummer muss größer als 0 sein.", nameof(collectionNumber));

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

        CollectionNumber = collectionNumber;

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
        int collectionNumber,
        int labelId,
        int? artistId,
        RecordFormat format,
        string albumName,
        int releaseYear,
        RecordCondition condition,
        string? information,
        Guid userId)
    {
        return new Record(
            0,
            collectionNumber,
            labelId,
            artistId,
            null,
            format,
            albumName,
            releaseYear,
            condition,
            information,
            userId);
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
            CollectionNumber,
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

    public Record SetAlbumCover(byte[] albumCover)
    {
        if (albumCover is null || albumCover.Length == 0)
            throw new ArgumentException("Das Album-Cover darf nicht leer sein.", nameof(albumCover));

        if (albumCover.Length > MaxAlbumCoverSizeBytes)
            throw new ArgumentException(
                $"Das Album-Cover darf höchstens {MaxAlbumCoverSizeBytes / (1024 * 1024)} MB groß sein.",
                nameof(albumCover));

        if (DetectAlbumCoverContentType(albumCover) is null)
            throw new ArgumentException(
                "Das Album-Cover muss im JPEG- oder PNG-Format vorliegen.",
                nameof(albumCover));

        return new Record(
            Id,
            CollectionNumber,
            LabelId,
            ArtistId,
            albumCover,
            Format,
            AlbumName,
            ReleaseYear,
            Condition,
            Information,
            UserId);
    }

    public static string? DetectAlbumCoverContentType(byte[] data)
    {
        if (HasSignature(data, _jpegSignature))
            return "image/jpeg";

        if (HasSignature(data, _pngSignature))
            return "image/png";

        return null;
    }

    private static bool HasSignature(byte[] data, byte[] signature)
    {
        return data.Length >= signature.Length && data.AsSpan(0, signature.Length).SequenceEqual(signature);
    }
}
