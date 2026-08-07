namespace MyMusic.Application.Features.Sammlung.Record.ResponseDtos;

public sealed record RecordResponse(
    int Id,
    int LabelId,
    string LabelName,
    int? ArtistId,
    string? ArtistName,
    RecordFormat Format,
    string AlbumName,
    int ReleaseYear,
    RecordCondition Condition,
    string? Information,
    string? AlbumCoverDataUrl,
    IReadOnlyList<RecordTrackResponse> Tracks);
