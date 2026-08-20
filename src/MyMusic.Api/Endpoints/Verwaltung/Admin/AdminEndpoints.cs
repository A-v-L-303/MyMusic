namespace MyMusic.Api.Endpoints.Verwaltung.Admin;

public static class AdminEndpoints
{
    public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/admin").RequireAuthorization("Admin");

        group.MapGet("/users", GetPagedUsersAsync);

        group.MapDelete("/users/{id:guid}", DeleteUserAsync);

        return endpoints;
    }

    /// <summary>
    /// Gibt die registrierten Benutzer seitenweise zurück, optional gefiltert nach Benutzername,
    /// E-Mail (Teilstring) oder Benutzer-ID (exakt).
    /// </summary>
    private static async Task<UserListResponse> GetPagedUsersAsync(
        int? page,
        int? pageSize,
        string? search,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var normalizedPage = Math.Max(page ?? 1, 1);

        var normalizedPageSize = Math.Clamp(pageSize ?? 20, 1, 100);

        var normalizedSearch = string.IsNullOrWhiteSpace(search) ? null : search.Trim();

        var query = new GetPagedUsersQuery(normalizedPage, normalizedPageSize, normalizedSearch);

        return await mediator.SendAsync(query, cancellationToken);
    }

    /// <summary>
    /// Löscht einen Benutzer inklusive aller seiner App-Daten und seines Keycloak-Accounts.
    /// </summary>
    private static async Task<IResult> DeleteUserAsync(
        Guid id,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        await mediator.SendAsync(new DeleteUserCommand(id), cancellationToken);

        return Results.NoContent();
    }
}
