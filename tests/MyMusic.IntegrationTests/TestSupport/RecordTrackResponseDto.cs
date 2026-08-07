namespace MyMusic.IntegrationTests.TestSupport;

public sealed record RecordTrackResponseDto(
    int Id,
    int RecordId,
    int ArtistId,
    string ArtistName,
    int GenreId,
    string GenreName,
    string TrackName,
    string RecordSide,
    int TrackNumber,
    string? Information);
