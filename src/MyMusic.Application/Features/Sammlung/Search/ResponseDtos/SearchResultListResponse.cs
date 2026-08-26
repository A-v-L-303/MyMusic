namespace MyMusic.Application.Features.Sammlung.Search.ResponseDtos;

public sealed record SearchResultListResponse(
    IReadOnlyList<SearchResultResponse> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);
