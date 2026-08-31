namespace MyMusic.IntegrationTests.TestSupport;

public sealed record RecordResponseDto(
    int Id,
    int CollectionNumber,
    int LabelId,
    string LabelName,
    int? ArtistId,
    string? ArtistName,
    string Format,
    string AlbumName,
    int ReleaseYear,
    string Condition,
    string? Information,
    string? AlbumCoverDataUrl,
    IReadOnlyList<RecordTrackResponseDto> Tracks);
