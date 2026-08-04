namespace MyMusic.Application.Features.Stammdaten.Genre.ResponseDtos;

public sealed record GenreListResponse(
    IReadOnlyList<GenreResponse> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);
