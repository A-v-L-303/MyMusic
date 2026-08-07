namespace MyMusic.Domain.Tests.DomainModels.Stammdaten.Artist;

public class ArtistTests
{
    [Fact]
    public void Create_GueltigerName_SetztEigenschaften()
    {
        // arrange
        var userId = Guid.NewGuid();

        // act
        var artist = ArtistEntity.Create("Pink Floyd", userId);

        // assert
        Assert.Equal("Pink Floyd", artist.Name);
        Assert.Equal(userId, artist.UserId);
    }

    [Fact]
    public void Create_LeererName_WirftArgumentException()
    {
        // arrange
        var userId = Guid.NewGuid();

        // act
        var act = () => ArtistEntity.Create(string.Empty, userId);

        // assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Create_NameZuLang_WirftArgumentException()
    {
        // arrange
        var userId = Guid.NewGuid();

        var zuLangerName = new string('a', ArtistEntity.MaxNameLength + 1);

        // act
        var act = () => ArtistEntity.Create(zuLangerName, userId);

        // assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Create_NameZuKurz_WirftArgumentException()
    {
        // arrange
        var userId = Guid.NewGuid();

        var zuKurzerName = new string('a', ArtistEntity.MinNameLength - 1);

        // act
        var act = () => ArtistEntity.Create(zuKurzerName, userId);

        // assert
        Assert.Throws<ArgumentException>(act);
    }

    [Theory]
    [InlineData("AC/DC")]
    [InlineData("Guns N' Roses")]
    [InlineData("R.E.M.")]
    [InlineData("Earth Wind & Fire")]
    [InlineData("Sigur Rós")]
    [InlineData("Blink-182")]
    public void Create_NameMitErlaubtenSonderzeichen_SetztEigenschaften(string name)
    {
        // arrange
        var userId = Guid.NewGuid();

        // act
        var artist = ArtistEntity.Create(name, userId);

        // assert
        Assert.Equal(name, artist.Name);
    }

    [Theory]
    [InlineData("Panic! at the Disco")]
    [InlineData("Crosby, Stills & Nash")]
    [InlineData("Beyoncé (Solo)")]
    [InlineData("<script>")]
    public void Create_NameMitVerbotenemSonderzeichen_WirftArgumentException(string name)
    {
        // arrange
        var userId = Guid.NewGuid();

        // act
        var act = () => ArtistEntity.Create(name, userId);

        // assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Update_GibtNeueInstanzMitGeaendertemNamenZurueck_BehaeltIdUndUserId()
    {
        // arrange
        var userId = Guid.NewGuid();

        var artist = ArtistEntity.Create("Pink Floyd", userId);

        // act
        var updatedArtist = artist.Update("Genesis");

        // assert
        Assert.NotSame(artist, updatedArtist);
        Assert.Equal("Genesis", updatedArtist.Name);
        Assert.Equal(artist.Id, updatedArtist.Id);
        Assert.Equal(userId, updatedArtist.UserId);
    }
}
