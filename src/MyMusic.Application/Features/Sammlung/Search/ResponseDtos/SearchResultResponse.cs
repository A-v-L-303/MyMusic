namespace MyMusic.Application.Features.Sammlung.Search.ResponseDtos;

public sealed record SearchResultResponse(
    int Id,
    int CollectionNumber,
    int LabelId,
    string LabelName,
    int? ArtistId,
    string? ArtistName,
    RecordFormat Format,
    string AlbumName,
    int ReleaseYear,
    RecordCondition Condition,
    string? Information,
    string? AlbumCoverDataUrl);
