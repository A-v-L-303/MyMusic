namespace MyMusic.Application.Tests.Features.System.CurrentUser;

public class GetCurrentUserQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_GibtUserIdDesAngemeldetenBenutzersZurueck()
    {
        // arrange
        var expectedUserId = Guid.NewGuid();

        var currentUserService = Substitute.For<ICurrentUserService>();

        currentUserService.UserId.Returns(expectedUserId);

        var handler = new GetCurrentUserQueryHandler(currentUserService, new CurrentUserResponseBuilder());

        // act
        var response = await handler.HandleAsync(new GetCurrentUserQuery(), CancellationToken.None);

        // assert
        Assert.Equal(expectedUserId, response.UserId);
    }
}
