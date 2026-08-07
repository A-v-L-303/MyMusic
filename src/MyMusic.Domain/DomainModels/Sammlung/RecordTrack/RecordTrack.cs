namespace MyMusic.Domain.DomainModels.Sammlung.RecordTrack;

public sealed class RecordTrack
{
    public const int MinTrackNameLength = 1;

    public const int MaxTrackNameLength = 150;

    public const int MaxInformationLength = 255;

    public const int MaxRecordSideLength = 3;

    public const int MinTrackNumber = 1;

    public const string TrackNamePattern = @"^[\p{L}\p{N} \-&'./()]+$";

    public const string RecordSidePattern = @"^[\p{L}\p{N}]{1,3}$";

    public int Id { get; private init; }

    public int RecordId { get; private init; }

    public int ArtistId { get; private init; }

    public int GenreId { get; private init; }

    public string TrackName { get; private init; }

    public string RecordSide { get; private init; }

    public int TrackNumber { get; private init; }

    public string? Information { get; private init; }

    public Guid UserId { get; private init; }

    internal RecordTrack(
        int id,
        int recordId,
        int artistId,
        int genreId,
        string trackName,
        string recordSide,
        int trackNumber,
        string? information,
        Guid userId)
    {
        if (recordId <= 0)
            throw new ArgumentException("Der Record ist erforderlich.", nameof(recordId));

        if (artistId <= 0)
            throw new ArgumentException("Der Artist ist erforderlich.", nameof(artistId));

        if (genreId <= 0)
            throw new ArgumentException("Das Genre ist erforderlich.", nameof(genreId));

        if (string.IsNullOrWhiteSpace(trackName))
            throw new ArgumentException("Der Trackname darf nicht leer sein.", nameof(trackName));

        if (trackName.Length < MinTrackNameLength)
            throw new ArgumentException(
                $"Der Trackname muss mindestens {MinTrackNameLength} Zeichen lang sein.",
                nameof(trackName));

        if (trackName.Length > MaxTrackNameLength)
            throw new ArgumentException(
                $"Der Trackname darf höchstens {MaxTrackNameLength} Zeichen lang sein.",
                nameof(trackName));

        if (!Regex.IsMatch(trackName, TrackNamePattern))
            throw new ArgumentException(
                "Der Trackname darf nur Buchstaben, Zahlen, Leerzeichen sowie - & ' . / ( ) enthalten.",
                nameof(trackName));

        if (string.IsNullOrWhiteSpace(recordSide))
            throw new ArgumentException("Die Seite darf nicht leer sein.", nameof(recordSide));

        if (recordSide.Length > MaxRecordSideLength)
            throw new ArgumentException(
                $"Die Seite darf höchstens {MaxRecordSideLength} Zeichen lang sein.",
                nameof(recordSide));

        if (!Regex.IsMatch(recordSide, RecordSidePattern))
            throw new ArgumentException(
                "Die Seite darf nur Buchstaben oder Ziffern enthalten.",
                nameof(recordSide));

        if (trackNumber < MinTrackNumber)
            throw new ArgumentException(
                $"Die Tracknummer muss mindestens {MinTrackNumber} sein.",
                nameof(trackNumber));

        if (!string.IsNullOrEmpty(information) && information.Length > MaxInformationLength)
            throw new ArgumentException(
                $"Das Feld 'information' darf höchstens {MaxInformationLength} Zeichen lang sein.",
                nameof(information));

        Id = id;

        RecordId = recordId;

        ArtistId = artistId;

        GenreId = genreId;

        TrackName = trackName;

        RecordSide = recordSide;

        TrackNumber = trackNumber;

        Information = information;

        UserId = userId;
    }

    public static RecordTrack Create(
        int recordId,
        int artistId,
        int genreId,
        string trackName,
        string recordSide,
        int trackNumber,
        string? information,
        Guid userId)
    {
        return new RecordTrack(
            0,
            recordId,
            artistId,
            genreId,
            trackName,
            recordSide,
            trackNumber,
            information,
            userId);
    }

    public RecordTrack Update(
        int artistId,
        int genreId,
        string trackName,
        string recordSide,
        int trackNumber,
        string? information)
    {
        return new RecordTrack(
            Id,
            RecordId,
            artistId,
            genreId,
            trackName,
            recordSide,
            trackNumber,
            information,
            UserId);
    }
}
