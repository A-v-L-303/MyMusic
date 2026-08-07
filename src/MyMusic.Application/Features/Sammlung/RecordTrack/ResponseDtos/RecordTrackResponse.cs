namespace MyMusic.Application.Features.Sammlung.RecordTrack.ResponseDtos;

public sealed record RecordTrackResponse(
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
