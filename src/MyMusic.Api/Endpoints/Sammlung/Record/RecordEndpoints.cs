namespace MyMusic.Api.Endpoints.Sammlung.Record;

public static class RecordEndpoints
{
    public static IEndpointRouteBuilder MapRecordEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/records").RequireAuthorization();

        group.MapGet(string.Empty, GetPagedRecordsAsync);

        group.MapGet("/{id:int}", GetRecordByIdAsync);

        group.MapPost(string.Empty, CreateRecordAsync);

        group.MapPut("/{id:int}", UpdateRecordAsync);

        group.MapDelete("/{id:int}", DeleteRecordAsync);

        // Formularbindung erfordert seit .NET 8 explizites Opt-out von Antiforgery (ADR 0008);
        // unkritisch, da diese API ausschließlich JWT-Bearer statt Cookies nutzt.
        group.MapPost("/{id:int}/cover", UploadRecordCoverAsync).DisableAntiforgery();

        return endpoints;
    }

    /// <summary>
    /// Gibt die Records des angemeldeten Benutzers gefiltert, sortiert und seitenweise zurück.
    /// </summary>
    private static async Task<RecordListResponse> GetPagedRecordsAsync(
        int? page,
        int? pageSize,
        string? name,
        int? artistId,
        int? labelId,
        int? yearFrom,
        int? yearTo,
        int? countryId,
        string? sortBy,
        string? sortDirection,
        ICurrentUserService currentUserService,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var normalizedPage = Math.Max(page ?? 1, 1);

        var normalizedPageSize = Math.Clamp(pageSize ?? 20, 1, 100);

        var query = new GetPagedRecordsQuery(
            currentUserService.UserId,
            normalizedPage,
            normalizedPageSize,
            name,
            artistId,
            labelId,
            yearFrom,
            yearTo,
            countryId,
            sortBy ?? "name",
            sortDirection ?? "asc");

        return await mediator.SendAsync(query, cancellationToken);
    }

    /// <summary>
    /// Gibt einen einzelnen Record des angemeldeten Benutzers zurück.
    /// </summary>
    private static async Task<RecordResponse> GetRecordByIdAsync(
        int id,
        ICurrentUserService currentUserService,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var query = new GetRecordByIdQuery(id, currentUserService.UserId);

        return await mediator.SendAsync(query, cancellationToken);
    }

    /// <summary>
    /// Legt einen neuen Record für den angemeldeten Benutzer an.
    /// </summary>
    private static async Task<IResult> CreateRecordAsync(
        CreateRecordCommand command,
        ICurrentUserService currentUserService,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        command.UserId = currentUserService.UserId;

        var response = await mediator.SendAsync(command, cancellationToken);

        return Results.Created($"/api/records/{response.Id}", response);
    }

    /// <summary>
    /// Aktualisiert einen eigenen Record.
    /// </summary>
    private static async Task<RecordResponse> UpdateRecordAsync(
        int id,
        UpdateRecordCommand command,
        ICurrentUserService currentUserService,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        command.Id = id;

        command.UserId = currentUserService.UserId;

        return await mediator.SendAsync(command, cancellationToken);
    }

    /// <summary>
    /// Löscht einen eigenen Record.
    /// </summary>
    private static async Task<IResult> DeleteRecordAsync(
        int id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        await mediator.SendAsync(new DeleteRecordCommand(id), cancellationToken);

        return Results.NoContent();
    }

    /// <summary>
    /// Lädt ein Album-Cover für einen eigenen Record hoch.
    /// </summary>
    private static async Task<RecordResponse> UploadRecordCoverAsync(
        int id,
        [FromForm] IFormFile file,
        ICurrentUserService currentUserService,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        await using var memoryStream = new MemoryStream();

        await file.CopyToAsync(memoryStream, cancellationToken);

        var command = new UploadRecordCoverCommand
        {
            Id = id,
            UserId = currentUserService.UserId,
            FileContent = memoryStream.ToArray()
        };

        return await mediator.SendAsync(command, cancellationToken);
    }
}
