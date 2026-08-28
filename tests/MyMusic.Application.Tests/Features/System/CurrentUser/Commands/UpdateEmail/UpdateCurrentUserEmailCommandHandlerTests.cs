namespace MyMusic.Application.Tests.Features.System.CurrentUser.Commands.UpdateEmail;

public class UpdateCurrentUserEmailCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_GueltigeEmail_RuftKeycloakAdminClientAufUndLiefertTrue()
    {
        // arrange
        var userId = Guid.NewGuid();

        var keycloakAdminClient = Substitute.For<IKeycloakAdminClient>();

        var handler = new UpdateCurrentUserEmailCommandHandler(keycloakAdminClient, new ExceptionManager());

        var command = new UpdateCurrentUserEmailCommand { Email = "neu@example.com", UserId = userId };

        // act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // assert
        Assert.True(result);
        await keycloakAdminClient.Received(1).UpdateEmailAsync(userId, "neu@example.com", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_KeycloakLiefertConflict_WirftConflictException()
    {
        // arrange
        var keycloakAdminClient = Substitute.For<IKeycloakAdminClient>();

        keycloakAdminClient
            .UpdateEmailAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new HttpRequestException(
                "E-Mail bereits vergeben.", null, HttpStatusCode.Conflict)));

        var handler = new UpdateCurrentUserEmailCommandHandler(keycloakAdminClient, new ExceptionManager());

        var command = new UpdateCurrentUserEmailCommand { Email = "vergeben@example.com", UserId = Guid.NewGuid() };

        // act
        var act = () => handler.HandleAsync(command, CancellationToken.None);

        // assert: der Konflikt einer bereits vergebenen E-Mail-Adresse wird als
        // ConflictException übersetzt, nicht als roher HTTP-Fehler durchgereicht
        await Assert.ThrowsAsync<ConflictException>(act);
    }

    [Fact]
    public async Task HandleAsync_KeycloakLiefertAnderenFehler_ReichtExceptionUnveraendertDurch()
    {
        // arrange
        var keycloakAdminClient = Substitute.For<IKeycloakAdminClient>();

        keycloakAdminClient
            .UpdateEmailAsync(Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new HttpRequestException(
                "Keycloak nicht erreichbar.", null, HttpStatusCode.ServiceUnavailable)));

        var handler = new UpdateCurrentUserEmailCommandHandler(keycloakAdminClient, new ExceptionManager());

        var command = new UpdateCurrentUserEmailCommand { Email = "neu@example.com", UserId = Guid.NewGuid() };

        // act
        var act = () => handler.HandleAsync(command, CancellationToken.None);

        // assert: nur 409 wird übersetzt, alles andere bleibt roh und fällt im
        // GlobalExceptionHandler auf den generischen 500-Fall zurück (siehe ADR 0026)
        await Assert.ThrowsAsync<HttpRequestException>(act);
    }
}
