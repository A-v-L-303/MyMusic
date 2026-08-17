namespace MyMusic.Application.Common.Services;

public interface IKeycloakAdminClient
{
    Task<IReadOnlyList<KeycloakUserSummary>> GetUsersAsync(CancellationToken cancellationToken);

    Task DeleteUserAsync(Guid userId, CancellationToken cancellationToken);
}
