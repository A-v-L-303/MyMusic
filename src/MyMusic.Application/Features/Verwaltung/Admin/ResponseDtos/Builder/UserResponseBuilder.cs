namespace MyMusic.Application.Features.Verwaltung.Admin.ResponseDtos.Builder;

public sealed class UserResponseBuilder
{
    public UserResponse Build(KeycloakUserSummary user)
    {
        return new UserResponse(user.Id, user.Username, user.Email, user.IsAdmin ? "Admin" : "User");
    }

    public UserListResponse BuildPaged(
        IReadOnlyList<KeycloakUserSummary> users,
        int totalCount,
        int page,
        int pageSize)
    {
        var items = users.Select(Build).ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new UserListResponse(items, totalCount, page, pageSize, totalPages);
    }
}
