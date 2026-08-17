namespace MyMusic.Application.Features.Verwaltung.Admin.Queries.GetPaged;

public sealed class GetPagedUsersQueryHandler(
    IKeycloakAdminClient keycloakAdminClient,
    UserResponseBuilder responseBuilder)
    : IQueryHandler<GetPagedUsersQuery, UserListResponse>
{
    public async Task<UserListResponse> HandleAsync(GetPagedUsersQuery query, CancellationToken cancellationToken)
    {
        var users = await keycloakAdminClient.GetUsersAsync(cancellationToken);

        var sortedUsers = users
            .OrderBy(user => user.Username, StringComparer.InvariantCulture)
            .ToList();

        var pagedUsers = sortedUsers
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        return responseBuilder.BuildPaged(pagedUsers, sortedUsers.Count, query.Page, query.PageSize);
    }
}
