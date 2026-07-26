namespace MyMusic.Application.Tests.Features.System.CurrentUser;

public class GetCurrentUserQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_GibtUserIdDesAngemeldetenBenutzersZurueck()
    {
        var expectedUserId = Guid.NewGuid();

        var currentUserService = Substitute.For<ICurrentUserService>();

        currentUserService.UserId.Returns(expectedUserId);

        var handler = new GetCurrentUserQueryHandler(currentUserService, new CurrentUserResponseBuilder());

        var response = await handler.HandleAsync(new GetCurrentUserQuery(), CancellationToken.None);

        Assert.Equal(expectedUserId, response.UserId);
    }
}
