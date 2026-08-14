namespace MyMusic.Api.Endpoints.Stammdaten.Label;

public static class LabelEndpoints
{
    public static IEndpointRouteBuilder MapLabelEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/labels").RequireAuthorization();

        group.MapGet(string.Empty, GetPagedLabelsAsync);

        group.MapGet("/all", GetAllLabelsAsync);

        group.MapGet("/{id:int}", GetLabelByIdAsync);

        group.MapPost(string.Empty, CreateLabelAsync);

        group.MapPut("/{id:int}", UpdateLabelAsync);

        group.MapDelete("/{id:int}", DeleteLabelAsync);

        return endpoints;
    }

    /// <summary>
    /// Gibt die Labels des angemeldeten Benutzers gefiltert, sortiert und seitenweise zurück.
    /// </summary>
    private static async Task<LabelListResponse> GetPagedLabelsAsync(
        int? page,
        int? pageSize,
        string? name,
        int? countryId,
        ICurrentUserService currentUserService,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var normalizedPage = Math.Max(page ?? 1, 1);

        var normalizedPageSize = Math.Clamp(pageSize ?? 20, 1, 100);

        var query = new GetPagedLabelsQuery(
            currentUserService.UserId,
            normalizedPage,
            normalizedPageSize,
            name,
            countryId);

        return await mediator.SendAsync(query, cancellationToken);
    }

    /// <summary>
    /// Gibt die vollständige, alphabetisch sortierte Label-Liste des angemeldeten Benutzers zurück.
    /// </summary>
    private static async Task<IEnumerable<LabelResponse>> GetAllLabelsAsync(
        ICurrentUserService currentUserService,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        return await mediator.SendAsync(new GetAllLabelsQuery(currentUserService.UserId), cancellationToken);
    }

    /// <summary>
    /// Gibt ein einzelnes Label des angemeldeten Benutzers zurück.
    /// </summary>
    private static async Task<LabelResponse> GetLabelByIdAsync(
        int id,
        ICurrentUserService currentUserService,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetLabelByIdQuery(id, currentUserService.UserId);

        return await mediator.SendAsync(query, cancellationToken);
    }

    /// <summary>
    /// Legt ein neues Label für den angemeldeten Benutzer an.
    /// </summary>
    private static async Task<IResult> CreateLabelAsync(
        CreateLabelCommand command,
        ICurrentUserService currentUserService,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        command.UserId = currentUserService.UserId;

        var response = await mediator.SendAsync(command, cancellationToken);

        return Results.Created($"/api/labels/{response.Id}", response);
    }

    /// <summary>
    /// Aktualisiert Name, Herkunftsland oder Freitext eines eigenen Labels.
    /// </summary>
    private static async Task<LabelResponse> UpdateLabelAsync(
        int id,
        UpdateLabelCommand command,
        ICurrentUserService currentUserService,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        command.Id = id;

        command.UserId = currentUserService.UserId;

        return await mediator.SendAsync(command, cancellationToken);
    }

    /// <summary>
    /// Löscht ein eigenes Label.
    /// </summary>
    private static async Task<IResult> DeleteLabelAsync(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        await mediator.SendAsync(new DeleteLabelCommand(id), cancellationToken);

        return Results.NoContent();
    }
}
