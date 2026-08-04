namespace MyMusic.Application.Tests.Features.System.CurrentUser;

public class CurrentUserResponseBuilderTests
{
    [Fact]
    public void Build_MapptDieUebergebeneUserIdInDieResponse()
    {
        // arrange
        var userId = Guid.NewGuid();

        var builder = new CurrentUserResponseBuilder();

        // act
        var response = builder.Build(userId);

        // assert
        Assert.Equal(userId, response.UserId);
    }
}
