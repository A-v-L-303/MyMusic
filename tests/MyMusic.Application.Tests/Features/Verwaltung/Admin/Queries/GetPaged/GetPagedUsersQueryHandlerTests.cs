namespace MyMusic.Application.Tests.Features.Verwaltung.Admin.Queries.GetPaged;

public class GetPagedUsersQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_SortiertAlphabetischUndSeitetInSpeicher()
    {
        // arrange
        var users = new List<KeycloakUserSummary>
        {
            new(Guid.NewGuid(), "zoe", "zoe@example.com", IsAdmin: false),
            new(Guid.NewGuid(), "anna", "anna@example.com", IsAdmin: true),
            new(Guid.NewGuid(), "max", "max@example.com", IsAdmin: false)
        };

        var keycloakAdminClient = Substitute.For<IKeycloakAdminClient>();

        keycloakAdminClient.GetUsersAsync(Arg.Any<CancellationToken>()).Returns(users);

        var handler = new GetPagedUsersQueryHandler(keycloakAdminClient, new UserResponseBuilder());

        // act
        var response = await handler.HandleAsync(new GetPagedUsersQuery(Page: 1, PageSize: 2), CancellationToken.None);

        // assert
        Assert.Equal(3, response.TotalCount);
        Assert.Equal(2, response.TotalPages);
        Assert.Equal(2, response.Items.Count);
        Assert.Equal("anna", response.Items[0].Username);
        Assert.Equal("max", response.Items[1].Username);
    }

    [Fact]
    public async Task HandleAsync_ZweiteSeite_GibtRestlicheEintraegeZurueck()
    {
        // arrange
        var users = new List<KeycloakUserSummary>
        {
            new(Guid.NewGuid(), "zoe", "zoe@example.com", IsAdmin: false),
            new(Guid.NewGuid(), "anna", "anna@example.com", IsAdmin: true),
            new(Guid.NewGuid(), "max", "max@example.com", IsAdmin: false)
        };

        var keycloakAdminClient = Substitute.For<IKeycloakAdminClient>();

        keycloakAdminClient.GetUsersAsync(Arg.Any<CancellationToken>()).Returns(users);

        var handler = new GetPagedUsersQueryHandler(keycloakAdminClient, new UserResponseBuilder());

        // act
        var response = await handler.HandleAsync(new GetPagedUsersQuery(Page: 2, PageSize: 2), CancellationToken.None);

        // assert
        Assert.Single(response.Items);
        Assert.Equal("zoe", response.Items[0].Username);
    }
}
