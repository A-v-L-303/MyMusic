namespace MyMusic.Api.Endpoints.Stammdaten.Artist;

public static class ArtistEndpoints
{
    public static IEndpointRouteBuilder MapArtistEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/artists").RequireAuthorization();

        group.MapGet(string.Empty, GetPagedArtistsAsync);

        group.MapGet("/{id:int}", GetArtistByIdAsync);

        group.MapPost(string.Empty, CreateArtistAsync);

        group.MapPut("/{id:int}", UpdateArtistAsync);

        group.MapDelete("/{id:int}", DeleteArtistAsync);

        return endpoints;
    }

    /// <summary>
    /// Gibt die Artists des angemeldeten Benutzers gefiltert, sortiert und seitenweise zurück.
    /// </summary>
    private static async Task<ArtistListResponse> GetPagedArtistsAsync(
        int? page,
        int? pageSize,
        string? name,
        int? labelId,
        ICurrentUserService currentUserService,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var normalizedPage = Math.Max(page ?? 1, 1);

        var normalizedPageSize = Math.Clamp(pageSize ?? 20, 1, 100);

        var query = new GetPagedArtistsQuery(
            currentUserService.UserId, normalizedPage, normalizedPageSize, name, labelId);

        return await mediator.SendAsync(query, cancellationToken);
    }

    /// <summary>
    /// Gibt einen einzelnen Artist des angemeldeten Benutzers zurück.
    /// </summary>
    private static async Task<ArtistResponse> GetArtistByIdAsync(
        int id,
        ICurrentUserService currentUserService,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetArtistByIdQuery(id, currentUserService.UserId);

        return await mediator.SendAsync(query, cancellationToken);
    }

    /// <summary>
    /// Legt einen neuen Artist für den angemeldeten Benutzer an.
    /// </summary>
    private static async Task<IResult> CreateArtistAsync(
        CreateArtistCommand command,
        ICurrentUserService currentUserService,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        command.UserId = currentUserService.UserId;

        var response = await mediator.SendAsync(command, cancellationToken);

        return Results.Created($"/api/artists/{response.Id}", response);
    }

    /// <summary>
    /// Aktualisiert den Namen eines eigenen Artists.
    /// </summary>
    private static async Task<ArtistResponse> UpdateArtistAsync(
        int id,
        UpdateArtistCommand command,
        ICurrentUserService currentUserService,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        command.Id = id;

        command.UserId = currentUserService.UserId;

        return await mediator.SendAsync(command, cancellationToken);
    }

    /// <summary>
    /// Löscht einen eigenen Artist.
    /// </summary>
    private static async Task<IResult> DeleteArtistAsync(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        await mediator.SendAsync(new DeleteArtistCommand(id), cancellationToken);

        return Results.NoContent();
    }
}
