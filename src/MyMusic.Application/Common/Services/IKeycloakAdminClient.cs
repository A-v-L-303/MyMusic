namespace MyMusic.Application.Common.Services;

public interface IKeycloakAdminClient
{
    Task<IReadOnlyList<KeycloakUserSummary>> GetUsersAsync(CancellationToken cancellationToken);

    Task DeleteUserAsync(Guid userId, CancellationToken cancellationToken);

    Task UpdateEmailAsync(Guid userId, string newEmail, CancellationToken cancellationToken);

    Task ResetPasswordAsync(Guid userId, string newPassword, CancellationToken cancellationToken);
}
