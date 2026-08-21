namespace MyMusic.Application.Tests.Features.Integration.Discogs.Queries.Search;

public class SearchDiscogsQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_SuchbegriffZuKurz_WirftValidationException()
    {
        // arrange
        var discogsClient = Substitute.For<IDiscogsClient>();

        var handler = new SearchDiscogsQueryHandler(
            discogsClient, new DiscogsResponseBuilder(), new ExceptionManager());

        // act
        var act = () => handler.HandleAsync(new SearchDiscogsQuery("a"), CancellationToken.None);

        // assert
        await Assert.ThrowsAsync<ValidationException>(act);
    }

    [Fact]
    public async Task HandleAsync_LeererSuchbegriff_WirftValidationException()
    {
        // arrange
        var discogsClient = Substitute.For<IDiscogsClient>();

        var handler = new SearchDiscogsQueryHandler(
            discogsClient, new DiscogsResponseBuilder(), new ExceptionManager());

        // act
        var act = () => handler.HandleAsync(new SearchDiscogsQuery("   "), CancellationToken.None);

        // assert
        await Assert.ThrowsAsync<ValidationException>(act);
    }

    [Fact]
    public async Task HandleAsync_GueltigerSuchbegriff_MapptDiscogsErgebnisseAufResponse()
    {
        // arrange
        var results = new List<DiscogsSearchResult>
        {
            new(1, "Nevermind", 1991, "DGC", "https://example.com/thumb.jpg")
        };

        var discogsClient = Substitute.For<IDiscogsClient>();

        discogsClient.SearchAsync("Nevermind", Arg.Any<CancellationToken>()).Returns(results);

        var handler = new SearchDiscogsQueryHandler(
            discogsClient, new DiscogsResponseBuilder(), new ExceptionManager());

        // act
        var response = await handler.HandleAsync(new SearchDiscogsQuery("Nevermind"), CancellationToken.None);

        // assert
        var searchResult = Assert.Single(response);

        Assert.Equal(1, searchResult.Id);
        Assert.Equal("Nevermind", searchResult.Title);
        Assert.Equal(1991, searchResult.Year);
        Assert.Equal("DGC", searchResult.Label);
    }

    [Fact]
    public async Task HandleAsync_SuchbegriffMitUmgebendenLeerzeichen_WirdGetrimmtAnClientUebergeben()
    {
        // arrange
        var discogsClient = Substitute.For<IDiscogsClient>();

        discogsClient.SearchAsync("Nevermind", Arg.Any<CancellationToken>())
            .Returns(new List<DiscogsSearchResult>());

        var handler = new SearchDiscogsQueryHandler(
            discogsClient, new DiscogsResponseBuilder(), new ExceptionManager());

        // act
        await handler.HandleAsync(new SearchDiscogsQuery("  Nevermind  "), CancellationToken.None);

        // assert: wirft nicht, da der getrimmte Suchbegriff die Mindestlaenge erreicht
        await discogsClient.Received(1).SearchAsync("Nevermind", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task HandleAsync_DiscogsClientWirftHttpRequestException_WirftDiscogsUnavailableException()
    {
        // arrange
        var discogsClient = Substitute.For<IDiscogsClient>();

        discogsClient.SearchAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<IReadOnlyList<DiscogsSearchResult>>(
                new HttpRequestException("Discogs nicht erreichbar.")));

        var handler = new SearchDiscogsQueryHandler(
            discogsClient, new DiscogsResponseBuilder(), new ExceptionManager());

        // act
        var act = () => handler.HandleAsync(new SearchDiscogsQuery("Nevermind"), CancellationToken.None);

        // assert
        await Assert.ThrowsAsync<DiscogsUnavailableException>(act);
    }

    [Fact]
    public async Task HandleAsync_DiscogsClientWirftTaskCanceledExceptionOhneAbbruch_WirftDiscogsUnavailableException()
    {
        // arrange
        var discogsClient = Substitute.For<IDiscogsClient>();

        discogsClient.SearchAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<IReadOnlyList<DiscogsSearchResult>>(
                new TaskCanceledException("Discogs-Timeout.")));

        var handler = new SearchDiscogsQueryHandler(
            discogsClient, new DiscogsResponseBuilder(), new ExceptionManager());

        // act
        var act = () => handler.HandleAsync(new SearchDiscogsQuery("Nevermind"), CancellationToken.None);

        // assert: kein echter Abbruch ueber CancellationToken.None angefordert -> als Discogs-Fehler gewertet
        await Assert.ThrowsAsync<DiscogsUnavailableException>(act);
    }
}
