namespace MyMusic.Application.Tests.Features.Integration.Discogs.ResponseDtos.Builder;

public class DiscogsResponseBuilderTests
{
    private readonly DiscogsResponseBuilder _builder = new();

    [Fact]
    public void BuildSearchResult_MapptAlleFelder()
    {
        // arrange
        var searchResult = new DiscogsSearchResult(1, "Nevermind", 1991, "DGC", "https://example.com/thumb.jpg");

        // act
        var response = _builder.BuildSearchResult(searchResult);

        // assert
        Assert.Equal(1, response.Id);
        Assert.Equal("Nevermind", response.Title);
        Assert.Equal(1991, response.Year);
        Assert.Equal("DGC", response.Label);
        Assert.Equal("https://example.com/thumb.jpg", response.ThumbnailUrl);
    }

    [Fact]
    public void BuildSearchResult_OhneJahrUndLabel_LiefertNullWerte()
    {
        // arrange
        var searchResult = new DiscogsSearchResult(1, "Nevermind", null, null, null);

        // act
        var response = _builder.BuildSearchResult(searchResult);

        // assert
        Assert.Null(response.Year);
        Assert.Null(response.Label);
        Assert.Null(response.ThumbnailUrl);
    }

    [Fact]
    public void BuildRelease_MapptFormatsUndTracklistVollstaendig()
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
            [
                new DiscogsTrack("A1", "Smells Like Teen Spirit", "5:01", "Nirvana"),
                new DiscogsTrack("A2", "In Bloom", null, null),
            ]);

        // act
        var response = _builder.BuildRelease(release);

        // assert
        Assert.Equal(2, response.Tracklist.Count);
        Assert.Equal("Nirvana", response.Tracklist[0].Artist);
        Assert.Equal("A2", response.Tracklist[1].Position);
        Assert.Null(response.Tracklist[1].Duration);
        Assert.Null(response.Tracklist[1].Artist);
        var format = Assert.Single(response.Formats);
        Assert.Equal("Vinyl", format.Name);
        Assert.Equal(["LP", "Album"], format.Descriptions);
    }

    [Fact]
    public void BuildRelease_LeereListen_LiefertLeereResponseListen()
    {
        // arrange
        var release = new DiscogsRelease(1, "Nevermind", 1991, [], [], [], [], [], null, []);

        // act
        var response = _builder.BuildRelease(release);

        // assert
        Assert.Empty(response.Artists);
        Assert.Empty(response.Labels);
        Assert.Empty(response.Formats);
        Assert.Empty(response.Tracklist);
    }
}
