namespace MyMusic.Api.Endpoints.System.CurrentUser;

public static class MeEndpoints
{
    public static IEndpointRouteBuilder MapMeEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/me").RequireAuthorization();

        group.MapGet(string.Empty, GetCurrentUserAsync);

        group.MapPut("/email", UpdateCurrentUserEmailAsync);

        group.MapPut("/password", ChangeCurrentUserPasswordAsync);

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

    /// <summary>
    /// Ändert die E-Mail-Adresse des aktuell angemeldeten Benutzers.
    /// </summary>
    private static async Task<IResult> UpdateCurrentUserEmailAsync(
        UpdateCurrentUserEmailCommand command,
        ICurrentUserService currentUserService,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        command.UserId = currentUserService.UserId;

        await mediator.SendAsync(command, cancellationToken);

        return Results.NoContent();
    }

    /// <summary>
    /// Ändert das Passwort des aktuell angemeldeten Benutzers.
    /// </summary>
    private static async Task<IResult> ChangeCurrentUserPasswordAsync(
        ChangeCurrentUserPasswordCommand command,
        ICurrentUserService currentUserService,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        command.UserId = currentUserService.UserId;

        await mediator.SendAsync(command, cancellationToken);

        return Results.NoContent();
    }
}
