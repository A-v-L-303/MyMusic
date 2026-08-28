namespace MyMusic.Application.Tests.Features.System.CurrentUser.Commands.ChangePassword;

public class ChangeCurrentUserPasswordCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_GueltigesPasswort_RuftKeycloakAdminClientAufUndLiefertTrue()
    {
        // arrange
        var userId = Guid.NewGuid();

        var keycloakAdminClient = Substitute.For<IKeycloakAdminClient>();

        var handler = new ChangeCurrentUserPasswordCommandHandler(keycloakAdminClient);

        var command = new ChangeCurrentUserPasswordCommand { NewPassword = "einSicheresPasswort1", UserId = userId };

        // act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // assert
        Assert.True(result);
        await keycloakAdminClient.Received(1).ResetPasswordAsync(
            userId, "einSicheresPasswort1", Arg.Any<CancellationToken>());
    }
}
