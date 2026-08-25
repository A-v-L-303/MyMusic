namespace MyMusic.IntegrationTests.TestSupport;

public sealed record SearchResultResponseDto(
    int Id,
    int LabelId,
    string LabelName,
    int? ArtistId,
    string? ArtistName,
    string Format,
    string AlbumName,
    int ReleaseYear,
    string Condition,
    string? Information,
    string? AlbumCoverDataUrl);
