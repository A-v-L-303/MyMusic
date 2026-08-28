namespace MyMusic.Application.Features.System.CurrentUser.Commands.UpdateEmail;

public sealed class UpdateCurrentUserEmailCommandHandler(
    IKeycloakAdminClient keycloakAdminClient,
    ExceptionManager exceptionManager)
    : ICommandHandler<UpdateCurrentUserEmailCommand, bool>
{
    public async Task<bool> HandleAsync(UpdateCurrentUserEmailCommand command, CancellationToken cancellationToken)
    {
        try
        {
            await keycloakAdminClient.UpdateEmailAsync(command.UserId, command.Email, cancellationToken);
        }
        catch (HttpRequestException exception) when (exception.StatusCode == HttpStatusCode.Conflict)
        {
            throw exceptionManager.Conflict("Diese E-Mail-Adresse wird bereits von einem anderen Konto verwendet.");
        }

        return true;
    }
}
