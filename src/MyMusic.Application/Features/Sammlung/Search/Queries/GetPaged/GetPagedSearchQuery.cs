namespace MyMusic.Application.Features.Sammlung.Search.Queries.GetPaged;

public sealed record GetPagedSearchQuery(
    Guid UserId,
    int Page,
    int PageSize,
    string? Query) : IQuery<SearchResultListResponse>;
