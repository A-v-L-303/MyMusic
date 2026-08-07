namespace MyMusic.Domain.Tests.DomainModels.Sammlung.RecordTrack;

public class RecordTrackTests
{
    [Fact]
    public void Create_GueltigeWerte_SetztEigenschaften()
    {
        // arrange
        var userId = Guid.NewGuid();

        // act
        var track = RecordTrackEntity.Create(1, 2, 3, "Come Together", "A", 1, "Opener", userId);

        // assert
        Assert.Equal(1, track.RecordId);
        Assert.Equal(2, track.ArtistId);
        Assert.Equal(3, track.GenreId);
        Assert.Equal("Come Together", track.TrackName);
        Assert.Equal("A", track.RecordSide);
        Assert.Equal(1, track.TrackNumber);
        Assert.Equal("Opener", track.Information);
        Assert.Equal(userId, track.UserId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_UngueltigeRecordId_WirftArgumentException(int recordId)
    {
        // arrange
        var userId = Guid.NewGuid();

        // act
        var act = () => RecordTrackEntity.Create(recordId, 2, 3, "Come Together", "A", 1, null, userId);

        // assert
        Assert.Throws<ArgumentException>(act);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_UngueltigeArtistId_WirftArgumentException(int artistId)
    {
        // arrange
        var userId = Guid.NewGuid();

        // act
        var act = () => RecordTrackEntity.Create(1, artistId, 3, "Come Together", "A", 1, null, userId);

        // assert
        Assert.Throws<ArgumentException>(act);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_UngueltigeGenreId_WirftArgumentException(int genreId)
    {
        // arrange
        var userId = Guid.NewGuid();

        // act
        var act = () => RecordTrackEntity.Create(1, 2, genreId, "Come Together", "A", 1, null, userId);

        // assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Create_LeererTrackname_WirftArgumentException()
    {
        // arrange
        var userId = Guid.NewGuid();

        // act
        var act = () => RecordTrackEntity.Create(1, 2, 3, string.Empty, "A", 1, null, userId);

        // assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Create_TracknameZuLang_WirftArgumentException()
    {
        // arrange
        var userId = Guid.NewGuid();

        var zuLangerName = new string('a', RecordTrackEntity.MaxTrackNameLength + 1);

        // act
        var act = () => RecordTrackEntity.Create(1, 2, 3, zuLangerName, "A", 1, null, userId);

        // assert
        Assert.Throws<ArgumentException>(act);
    }

    [Theory]
    [InlineData("Come Together")]
    [InlineData("1989 (Taylor's Version)")]
    [InlineData("Ok Computer")]
    [InlineData("AC/DC Live")]
    [InlineData("Nevermind - Remastered")]
    public void Create_TracknameMitErlaubtenSonderzeichen_SetztEigenschaften(string trackName)
    {
        // arrange
        var userId = Guid.NewGuid();

        // act
        var track = RecordTrackEntity.Create(1, 2, 3, trackName, "A", 1, null, userId);

        // assert
        Assert.Equal(trackName, track.TrackName);
    }

    [Theory]
    [InlineData("Ro@ck")]
    [InlineData("<script>")]
    [InlineData("Track!")]
    public void Create_TracknameMitVerbotenemSonderzeichen_WirftArgumentException(string trackName)
    {
        // arrange
        var userId = Guid.NewGuid();

        // act
        var act = () => RecordTrackEntity.Create(1, 2, 3, trackName, "A", 1, null, userId);

        // assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Create_LeereRecordSide_WirftArgumentException()
    {
        // arrange
        var userId = Guid.NewGuid();

        // act
        var act = () => RecordTrackEntity.Create(1, 2, 3, "Come Together", string.Empty, 1, null, userId);

        // assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Create_RecordSideZuLang_WirftArgumentException()
    {
        // arrange
        var userId = Guid.NewGuid();

        // act
        var act = () => RecordTrackEntity.Create(1, 2, 3, "Come Together", "ABCD", 1, null, userId);

        // assert
        Assert.Throws<ArgumentException>(act);
    }

    [Theory]
    [InlineData("A")]
    [InlineData("B2")]
    [InlineData("0")]
    [InlineData("123")]
    public void Create_GueltigeRecordSide_SetztEigenschaften(string recordSide)
    {
        // arrange
        var userId = Guid.NewGuid();

        // act
        var track = RecordTrackEntity.Create(1, 2, 3, "Come Together", recordSide, 1, null, userId);

        // assert
        Assert.Equal(recordSide, track.RecordSide);
    }

    [Theory]
    [InlineData("A-")]
    [InlineData("A.")]
    [InlineData("A B")]
    public void Create_RecordSideMitSonderzeichen_WirftArgumentException(string recordSide)
    {
        // arrange
        var userId = Guid.NewGuid();

        // act
        var act = () => RecordTrackEntity.Create(1, 2, 3, "Come Together", recordSide, 1, null, userId);

        // assert
        Assert.Throws<ArgumentException>(act);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_UngueltigeTrackNumber_WirftArgumentException(int trackNumber)
    {
        // arrange
        var userId = Guid.NewGuid();

        // act
        var act = () => RecordTrackEntity.Create(1, 2, 3, "Come Together", "A", trackNumber, null, userId);

        // assert
        Assert.Throws<ArgumentException>(act);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Create_InformationNullOderLeer_SetztEigenschaften(string? information)
    {
        // arrange
        var userId = Guid.NewGuid();

        // act
        var track = RecordTrackEntity.Create(1, 2, 3, "Come Together", "A", 1, information, userId);

        // assert
        Assert.Equal(information, track.Information);
    }

    [Fact]
    public void Create_InformationZuLang_WirftArgumentException()
    {
        // arrange
        var userId = Guid.NewGuid();

        var zuLangeInformation = new string('a', RecordTrackEntity.MaxInformationLength + 1);

        // act
        var act = () => RecordTrackEntity.Create(1, 2, 3, "Come Together", "A", 1, zuLangeInformation, userId);

        // assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Update_GibtNeueInstanzMitGeaendertenWertenZurueck_BehaeltIdRecordIdUndUserId()
    {
        // arrange
        var userId = Guid.NewGuid();

        var track = RecordTrackEntity.Create(1, 2, 3, "Come Together", "A", 1, null, userId);

        // act
        var updatedTrack = track.Update(4, 5, "Something", "B", 2, "Zweiter Track");

        // assert
        Assert.NotSame(track, updatedTrack);
        Assert.Equal(4, updatedTrack.ArtistId);
        Assert.Equal(5, updatedTrack.GenreId);
        Assert.Equal("Something", updatedTrack.TrackName);
        Assert.Equal("B", updatedTrack.RecordSide);
        Assert.Equal(2, updatedTrack.TrackNumber);
        Assert.Equal("Zweiter Track", updatedTrack.Information);
        Assert.Equal(track.Id, updatedTrack.Id);
        Assert.Equal(track.RecordId, updatedTrack.RecordId);
        Assert.Equal(userId, updatedTrack.UserId);
    }
}
