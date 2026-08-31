namespace MyMusic.Application.Tests.Features.Integration.Discogs.Queries.GetRelease;

public class GetDiscogsReleaseQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_GueltigeId_MapptVollstaendigesReleaseAufResponse()
    {
        // arrange
        var release = new DiscogsRelease(
            1,
            "Nevermind",
            1991,
            ["Nirvana"],
            ["DGC"],
            ["Rock"],
            ["Grunge"],
            [new DiscogsFormat("Vinyl", ["LP", "Album"])],
            "https://example.com/cover.jpg",
            [new DiscogsTrack("A1", "Smells Like Teen Spirit", "5:01", "Nirvana")],
            "US");

        var discogsClient = Substitute.For<IDiscogsClient>();

        discogsClient.GetReleaseAsync(1, Arg.Any<CancellationToken>()).Returns(release);

        var handler = new GetDiscogsReleaseQueryHandler(
            discogsClient, new DiscogsResponseBuilder(), new ExceptionManager());

        // act
        var response = await handler.HandleAsync(new GetDiscogsReleaseQuery(1), CancellationToken.None);

        // assert
        Assert.Equal("Nevermind", response.Title);
        Assert.Equal(1991, response.Year);
        Assert.Equal(["Nirvana"], response.Artists);
        Assert.Equal(["DGC"], response.Labels);
        Assert.Equal(["Rock"], response.Genres);
        Assert.Equal(["Grunge"], response.Styles);
        Assert.Equal("https://example.com/cover.jpg", response.CoverImageUrl);
        var format = Assert.Single(response.Formats);
        Assert.Equal("Vinyl", format.Name);
        Assert.Equal(["LP", "Album"], format.Descriptions);
        var track = Assert.Single(response.Tracklist);
        Assert.Equal("A1", track.Position);
        Assert.Equal("Smells Like Teen Spirit", track.Title);
        Assert.Equal("5:01", track.Duration);
        Assert.Equal("Nirvana", track.Artist);
        Assert.Equal("US", response.Country);
    }

    [Fact]
    public async Task HandleAsync_ReleaseOhneCover_LiefertNullAlsCoverImageUrl()
    {
        // arrange
        var release = new DiscogsRelease(
            1, "Nevermind", 1991, ["Nirvana"], ["DGC"], [], [], [], null, [], null);

        var discogsClient = Substitute.For<IDiscogsClient>();

        discogsClient.GetReleaseAsync(1, Arg.Any<CancellationToken>()).Returns(release);

        var handler = new GetDiscogsReleaseQueryHandler(
            discogsClient, new DiscogsResponseBuilder(), new ExceptionManager());

        // act
        var response = await handler.HandleAsync(new GetDiscogsReleaseQuery(1), CancellationToken.None);

        // assert
        Assert.Null(response.CoverImageUrl);
        Assert.Empty(response.Formats);
        Assert.Empty(response.Tracklist);
    }

    [Fact]
    public async Task HandleAsync_DiscogsClientWirftHttpRequestException_WirftDiscogsUnavailableException()
    {
        // arrange
        var discogsClient = Substitute.For<IDiscogsClient>();

        discogsClient.GetReleaseAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<DiscogsRelease>(new HttpRequestException("Discogs nicht erreichbar.")));

        var handler = new GetDiscogsReleaseQueryHandler(
            discogsClient, new DiscogsResponseBuilder(), new ExceptionManager());

        // act
        var act = () => handler.HandleAsync(new GetDiscogsReleaseQuery(1), CancellationToken.None);

        // assert: gilt auch fuer eine unbekannte Discogs-Release-Id (Discogs antwortet 404),
        // die einheitlich als Discogs-Fehler behandelt wird, nicht als MyMusic-eigenes 404
        await Assert.ThrowsAsync<DiscogsUnavailableException>(act);
    }
}
