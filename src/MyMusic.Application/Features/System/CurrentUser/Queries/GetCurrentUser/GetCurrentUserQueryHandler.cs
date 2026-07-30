namespace MyMusic.Application.Features.System.CurrentUser.Queries.GetCurrentUser;

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
