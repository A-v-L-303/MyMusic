namespace MyMusic.Application.Features.System.CurrentUser.Queries.GetCurrentUser;

/// <summary>Setzt <see cref="GetCurrentUserQuery"/> um: liest die Benutzer-ID aus dem aktuellen JWT.</summary>
public sealed class GetCurrentUserQueryHandler(
    ICurrentUserService currentUserService,
    CurrentUserResponseBuilder responseBuilder)
    : IQueryHandler<GetCurrentUserQuery, CurrentUserResponse>
{
    public Task<CurrentUserResponse> HandleAsync(GetCurrentUserQuery query, CancellationToken cancellationToken)
    {
        return Task.FromResult(responseBuilder.Build(currentUserService.UserId));
    }
}
