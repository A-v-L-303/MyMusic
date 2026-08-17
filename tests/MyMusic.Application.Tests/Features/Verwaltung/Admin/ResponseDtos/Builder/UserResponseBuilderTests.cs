namespace MyMusic.Application.Tests.Features.Verwaltung.Admin.ResponseDtos.Builder;

public class UserResponseBuilderTests
{
    private readonly UserResponseBuilder _builder = new();

    [Fact]
    public void Build_MitAdminRolle_MapptRolleAlsAdmin()
    {
        // arrange
        var user = new KeycloakUserSummary(Guid.NewGuid(), "max", "max@example.com", IsAdmin: true);

        // act
        var response = _builder.Build(user);

        // assert
        Assert.Equal(user.Id, response.Id);
        Assert.Equal("max", response.Username);
        Assert.Equal("max@example.com", response.Email);
        Assert.Equal("Admin", response.Role);
    }

    [Fact]
    public void Build_OhneAdminRolle_MapptRolleAlsUser()
    {
        // arrange
        var user = new KeycloakUserSummary(Guid.NewGuid(), "erika", "erika@example.com", IsAdmin: false);

        // act
        var response = _builder.Build(user);

        // assert
        Assert.Equal("User", response.Role);
    }

    [Fact]
    public void BuildPaged_MapptItemsUndBerechnetGesamtseitenzahl()
    {
        // arrange
        var users = new List<KeycloakUserSummary>
        {
            new(Guid.NewGuid(), "max", "max@example.com", IsAdmin: true)
        };

        // act
        var response = _builder.BuildPaged(users, totalCount: 25, page: 2, pageSize: 10);

        // assert
        Assert.Single(response.Items);
        Assert.Equal("max", response.Items[0].Username);
        Assert.Equal(25, response.TotalCount);
        Assert.Equal(2, response.Page);
        Assert.Equal(10, response.PageSize);
        Assert.Equal(3, response.TotalPages);
    }
}
