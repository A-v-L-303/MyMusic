namespace MyMusic.Api;

public static class MeEndpoints
{
    /// <summary>Registriert den Endpunkt <c>GET /api/me</c> zum Abfragen der eigenen Benutzerdaten.</summary>
    public static IEndpointRouteBuilder MapMeEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/me").RequireAuthorization();

        group.MapGet(string.Empty, GetCurrentUserAsync);

        return endpoints;
    }

    private static async Task<CurrentUserResponse> GetCurrentUserAsync(
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        return await mediator.SendAsync(new GetCurrentUserQuery(), cancellationToken);
    }
}
