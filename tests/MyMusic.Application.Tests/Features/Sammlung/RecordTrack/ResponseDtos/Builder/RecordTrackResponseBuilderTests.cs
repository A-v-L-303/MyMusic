namespace MyMusic.Application.Tests.Features.Sammlung.RecordTrack.ResponseDtos.Builder;

public class RecordTrackResponseBuilderTests
{
    private readonly RecordTrackResponseBuilder _builder = new();

    [Fact]
    public void Build_MapptAlleFelderInklusiveArtistUndGenreNamen()
    {
        // arrange
        var track = RecordTrackEntity.Create(1, 2, 3, "Come Together", "A", 1, "Opener", Guid.NewGuid());

        // act
        var response = _builder.Build(track, "The Beatles", "Rock");

        // assert
        Assert.Equal(track.Id, response.Id);
        Assert.Equal(1, response.RecordId);
        Assert.Equal(2, response.ArtistId);
        Assert.Equal("The Beatles", response.ArtistName);
        Assert.Equal(3, response.GenreId);
        Assert.Equal("Rock", response.GenreName);
        Assert.Equal("Come Together", response.TrackName);
        Assert.Equal("A", response.RecordSide);
        Assert.Equal(1, response.TrackNumber);
        Assert.Equal("Opener", response.Information);
    }
}
