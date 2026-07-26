namespace MyMusic.Application.Tests.Features.System.CurrentUser;

public class CurrentUserResponseBuilderTests
{
    [Fact]
    public void Build_MapptDieUebergebeneUserIdInDieResponse()
    {
        var userId = Guid.NewGuid();

        var builder = new CurrentUserResponseBuilder();

        var response = builder.Build(userId);

        Assert.Equal(userId, response.UserId);
    }
}
