namespace MyMusic.Api.Endpoints.Stammdaten.Genre;

public static class GenreEndpoints
{
    public static IEndpointRouteBuilder MapGenreEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/genres").RequireAuthorization();

        group.MapGet(string.Empty, GetPagedGenresAsync);

        group.MapGet("/all", GetAllGenresAsync);

        group.MapGet("/{id:int}", GetGenreByIdAsync);

        group.MapPost(string.Empty, CreateGenreAsync);

        group.MapPut("/{id:int}", UpdateGenreAsync);

        group.MapDelete("/{id:int}", DeleteGenreAsync);

        return endpoints;
    }

    /// <summary>
    /// Gibt die Genres des angemeldeten Benutzers gefiltert, sortiert und seitenweise zurück.
    /// </summary>
    private static async Task<GenreListResponse> GetPagedGenresAsync(
        int? page,
        int? pageSize,
        string? name,
        ICurrentUserService currentUserService,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var normalizedPage = Math.Max(page ?? 1, 1);

        var normalizedPageSize = Math.Clamp(pageSize ?? 20, 1, 100);

        var query = new GetPagedGenresQuery(currentUserService.UserId, normalizedPage, normalizedPageSize, name);

        return await mediator.SendAsync(query, cancellationToken);
    }

    /// <summary>
    /// Gibt die vollständige, alphabetisch sortierte Genre-Liste des angemeldeten Benutzers zurück.
    /// </summary>
    private static async Task<IEnumerable<GenreResponse>> GetAllGenresAsync(
        ICurrentUserService currentUserService,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        return await mediator.SendAsync(new GetAllGenresQuery(currentUserService.UserId), cancellationToken);
    }

    /// <summary>
    /// Gibt ein einzelnes Genre des angemeldeten Benutzers zurück.
    /// </summary>
    private static async Task<GenreResponse> GetGenreByIdAsync(
        int id,
        ICurrentUserService currentUserService,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetGenreByIdQuery(id, currentUserService.UserId);

        return await mediator.SendAsync(query, cancellationToken);
    }

    /// <summary>
    /// Legt ein neues Genre für den angemeldeten Benutzer an.
    /// </summary>
    private static async Task<IResult> CreateGenreAsync(
        CreateGenreCommand command,
        ICurrentUserService currentUserService,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        command.UserId = currentUserService.UserId;

        var response = await mediator.SendAsync(command, cancellationToken);

        return Results.Created($"/api/genres/{response.Id}", response);
    }

    /// <summary>
    /// Aktualisiert den Namen eines eigenen Genres.
    /// </summary>
    private static async Task<GenreResponse> UpdateGenreAsync(
        int id,
        UpdateGenreCommand command,
        ICurrentUserService currentUserService,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        command.Id = id;

        command.UserId = currentUserService.UserId;

        return await mediator.SendAsync(command, cancellationToken);
    }

    /// <summary>
    /// Löscht ein eigenes Genre.
    /// </summary>
    private static async Task<IResult> DeleteGenreAsync(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        await mediator.SendAsync(new DeleteGenreCommand(id), cancellationToken);

        return Results.NoContent();
    }
}
