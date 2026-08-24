namespace MyMusic.Api.Endpoints.Sammlung.Dashboard;

public static class DashboardEndpoints
{
    public static IEndpointRouteBuilder MapDashboardEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/dashboard").RequireAuthorization();

        group.MapGet(string.Empty, GetDashboardAsync);

        return endpoints;
    }

    /// <summary>
    /// Gibt aggregierte Statistiken zur Sammlung des angemeldeten Benutzers zurück.
    /// </summary>
    private static async Task<DashboardResponse> GetDashboardAsync(
        ICurrentUserService currentUserService,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        return await mediator.SendAsync(new GetDashboardQuery(currentUserService.UserId), cancellationToken);
    }
}
