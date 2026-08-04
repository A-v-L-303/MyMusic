namespace MyMusic.Api.Endpoints.System.CurrentUser;

public static class MeEndpoints
{
    public static IEndpointRouteBuilder MapMeEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/me").RequireAuthorization();

        group.MapGet(string.Empty, GetCurrentUserAsync);

        return endpoints;
    }

    /// <summary>
    /// Gibt die Id des aktuell angemeldeten Benutzers zurück.
    /// </summary>
    private static async Task<CurrentUserResponse> GetCurrentUserAsync(
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        return await mediator.SendAsync(new GetCurrentUserQuery(), cancellationToken);
    }
}
