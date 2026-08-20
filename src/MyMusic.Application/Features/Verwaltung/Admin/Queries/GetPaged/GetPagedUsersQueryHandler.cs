namespace MyMusic.Application.Features.Verwaltung.Admin.Queries.GetPaged;

public sealed class GetPagedUsersQueryHandler(
    IKeycloakAdminClient keycloakAdminClient,
    UserResponseBuilder responseBuilder)
    : IQueryHandler<GetPagedUsersQuery, UserListResponse>
{
    public async Task<UserListResponse> HandleAsync(GetPagedUsersQuery query, CancellationToken cancellationToken)
    {
        var users = await keycloakAdminClient.GetUsersAsync(cancellationToken);

        var filteredUsers = FilterBySearch(users, query.Search);

        var sortedUsers = filteredUsers
            .OrderBy(user => user.Username, StringComparer.InvariantCulture)
            .ToList();

        var pagedUsers = sortedUsers
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        return responseBuilder.BuildPaged(pagedUsers, sortedUsers.Count, query.Page, query.PageSize);
    }

    private static IReadOnlyList<KeycloakUserSummary> FilterBySearch(
        IReadOnlyList<KeycloakUserSummary> users,
        string? search)
    {
        if (search is null)
            return users;

        if (Guid.TryParse(search, out var userId))
            return users.Where(user => user.Id == userId).ToList();

        return users
            .Where(user =>
                user.Username.Contains(search, StringComparison.OrdinalIgnoreCase)
                || user.Email.Contains(search, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }
}
