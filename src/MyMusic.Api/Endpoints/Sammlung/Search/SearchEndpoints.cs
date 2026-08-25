namespace MyMusic.Api.Endpoints.Sammlung.Search;

public static class SearchEndpoints
{
    public static IEndpointRouteBuilder MapSearchEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/search").RequireAuthorization();

        group.MapGet(string.Empty, GetPagedSearchAsync);

        return endpoints;
    }

    /// <summary>
    /// Durchsucht die Records des angemeldeten Benutzers über Titel, Artist, Label, Genre und Land.
    /// </summary>
    private static async Task<SearchResultListResponse> GetPagedSearchAsync(
        string? q,
        int? page,
        int? pageSize,
        ICurrentUserService currentUserService,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var normalizedPage = Math.Max(page ?? 1, 1);

        var normalizedPageSize = Math.Clamp(pageSize ?? 20, 1, 100);

        var query = new GetPagedSearchQuery(currentUserService.UserId, normalizedPage, normalizedPageSize, q);

        return await mediator.SendAsync(query, cancellationToken);
    }
}
