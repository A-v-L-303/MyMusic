namespace MyMusic.Application.Features.System.CurrentUser.Commands.ChangePassword;

public sealed class ChangeCurrentUserPasswordCommandHandler(IKeycloakAdminClient keycloakAdminClient)
    : ICommandHandler<ChangeCurrentUserPasswordCommand, bool>
{
    public async Task<bool> HandleAsync(ChangeCurrentUserPasswordCommand command, CancellationToken cancellationToken)
    {
        await keycloakAdminClient.ResetPasswordAsync(command.UserId, command.NewPassword, cancellationToken);

        return true;
    }
}
