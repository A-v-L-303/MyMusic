namespace MyMusic.Application.Features.Stammdaten.Artist.ResponseDtos;

public sealed record ArtistListResponse(
    IReadOnlyList<ArtistResponse> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);
