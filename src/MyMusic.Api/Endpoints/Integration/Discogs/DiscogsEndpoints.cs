namespace MyMusic.Api.Endpoints.Integration.Discogs;

public static class DiscogsEndpoints
{
    public static IEndpointRouteBuilder MapDiscogsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/discogs").RequireAuthorization();

        group.MapGet("/search", SearchAsync);

        group.MapGet("/releases/{id:int}", GetReleaseAsync);

        return endpoints;
    }

    /// <summary>
    /// Sucht in der Discogs-Datenbank nach Metadaten-Treffern (Kurzdaten, unpaginiert).
    /// </summary>
    private static async Task<IEnumerable<DiscogsSearchResultResponse>> SearchAsync(
        string? q,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new SearchDiscogsQuery(q ?? string.Empty);

        return await mediator.SendAsync(query, cancellationToken);
    }

    /// <summary>
    /// Ruft die vollständigen Metadaten eines Discogs-Release ab (Cover, Tracklist, Format).
    /// </summary>
    private static async Task<DiscogsReleaseResponse> GetReleaseAsync(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        return await mediator.SendAsync(new GetDiscogsReleaseQuery(id), cancellationToken);
    }
}
