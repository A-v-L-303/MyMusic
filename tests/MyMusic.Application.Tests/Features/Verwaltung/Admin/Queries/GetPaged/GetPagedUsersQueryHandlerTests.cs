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
        var response = await handler.HandleAsync(
            new GetPagedUsersQuery(Page: 1, PageSize: 2, Search: null),
            CancellationToken.None);

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
        var response = await handler.HandleAsync(
            new GetPagedUsersQuery(Page: 2, PageSize: 2, Search: null),
            CancellationToken.None);

        // assert
        Assert.Single(response.Items);
        Assert.Equal("zoe", response.Items[0].Username);
    }

    [Fact]
    public async Task HandleAsync_SuchtNachTeilstringDesBenutzernamens_GibtNurTreffer()
    {
        // arrange
        var users = new List<KeycloakUserSummary>
        {
            new(Guid.NewGuid(), "maxine", "maxine@example.com", IsAdmin: false),
            new(Guid.NewGuid(), "anna", "anna@example.com", IsAdmin: true),
            new(Guid.NewGuid(), "max", "max@example.com", IsAdmin: false)
        };

        var keycloakAdminClient = Substitute.For<IKeycloakAdminClient>();

        keycloakAdminClient.GetUsersAsync(Arg.Any<CancellationToken>()).Returns(users);

        var handler = new GetPagedUsersQueryHandler(keycloakAdminClient, new UserResponseBuilder());

        // act
        var response = await handler.HandleAsync(
            new GetPagedUsersQuery(Page: 1, PageSize: 20, Search: "max"),
            CancellationToken.None);

        // assert
        Assert.Equal(2, response.TotalCount);
        Assert.Equal("max", response.Items[0].Username);
        Assert.Equal("maxine", response.Items[1].Username);
    }

    [Fact]
    public async Task HandleAsync_SuchtNachTeilstringDerEmail_GibtNurTreffer()
    {
        // arrange
        var users = new List<KeycloakUserSummary>
        {
            new(Guid.NewGuid(), "anna", "anna@example.com", IsAdmin: true),
            new(Guid.NewGuid(), "max", "max@other.com", IsAdmin: false)
        };

        var keycloakAdminClient = Substitute.For<IKeycloakAdminClient>();

        keycloakAdminClient.GetUsersAsync(Arg.Any<CancellationToken>()).Returns(users);

        var handler = new GetPagedUsersQueryHandler(keycloakAdminClient, new UserResponseBuilder());

        // act
        var response = await handler.HandleAsync(
            new GetPagedUsersQuery(Page: 1, PageSize: 20, Search: "example.com"),
            CancellationToken.None);

        // assert
        Assert.Single(response.Items);
        Assert.Equal("anna", response.Items[0].Username);
    }

    [Fact]
    public async Task HandleAsync_SuchtGrossKleinschreibungsunabhaengig()
    {
        // arrange
        var users = new List<KeycloakUserSummary>
        {
            new(Guid.NewGuid(), "Anna", "anna@example.com", IsAdmin: true)
        };

        var keycloakAdminClient = Substitute.For<IKeycloakAdminClient>();

        keycloakAdminClient.GetUsersAsync(Arg.Any<CancellationToken>()).Returns(users);

        var handler = new GetPagedUsersQueryHandler(keycloakAdminClient, new UserResponseBuilder());

        // act
        var response = await handler.HandleAsync(
            new GetPagedUsersQuery(Page: 1, PageSize: 20, Search: "ANNA"),
            CancellationToken.None);

        // assert
        Assert.Single(response.Items);
    }

    [Fact]
    public async Task HandleAsync_SuchtNachVollstaendigerBenutzerId_GibtGenauDiesenBenutzerZurueck()
    {
        // arrange
        var targetId = Guid.NewGuid();

        var users = new List<KeycloakUserSummary>
        {
            new(targetId, "anna", "anna@example.com", IsAdmin: true),
            new(Guid.NewGuid(), "max", "max@example.com", IsAdmin: false)
        };

        var keycloakAdminClient = Substitute.For<IKeycloakAdminClient>();

        keycloakAdminClient.GetUsersAsync(Arg.Any<CancellationToken>()).Returns(users);

        var handler = new GetPagedUsersQueryHandler(keycloakAdminClient, new UserResponseBuilder());

        // act
        var response = await handler.HandleAsync(
            new GetPagedUsersQuery(Page: 1, PageSize: 20, Search: targetId.ToString()),
            CancellationToken.None);

        // assert
        Assert.Single(response.Items);
        Assert.Equal("anna", response.Items[0].Username);
    }

    [Fact]
    public async Task HandleAsync_SucheOhneTreffer_GibtLeereListeZurueck()
    {
        // arrange
        var users = new List<KeycloakUserSummary>
        {
            new(Guid.NewGuid(), "anna", "anna@example.com", IsAdmin: true)
        };

        var keycloakAdminClient = Substitute.For<IKeycloakAdminClient>();

        keycloakAdminClient.GetUsersAsync(Arg.Any<CancellationToken>()).Returns(users);

        var handler = new GetPagedUsersQueryHandler(keycloakAdminClient, new UserResponseBuilder());

        // act
        var response = await handler.HandleAsync(
            new GetPagedUsersQuery(Page: 1, PageSize: 20, Search: "unbekannt"),
            CancellationToken.None);

        // assert
        Assert.Empty(response.Items);
        Assert.Equal(0, response.TotalCount);
        Assert.Equal(0, response.TotalPages);
    }
}
