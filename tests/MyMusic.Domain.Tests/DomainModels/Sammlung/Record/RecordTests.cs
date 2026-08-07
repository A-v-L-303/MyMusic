namespace MyMusic.Domain.Tests.DomainModels.Sammlung.Record;

public class RecordTests
{
    private static readonly int _currentYear = DateTime.UtcNow.Year;

    [Fact]
    public void Create_GueltigeWerte_SetztEigenschaften()
    {
        // arrange
        var userId = Guid.NewGuid();

        // act
        var record = RecordEntity.Create(
            1, 2, RecordFormat.Album, "Abbey Road", 1969, RecordCondition.Nm, "Erste Pressung", userId);

        // assert
        Assert.Equal(1, record.LabelId);
        Assert.Equal(2, record.ArtistId);
        Assert.Null(record.AlbumCover);
        Assert.Equal(RecordFormat.Album, record.Format);
        Assert.Equal("Abbey Road", record.AlbumName);
        Assert.Equal(1969, record.ReleaseYear);
        Assert.Equal(RecordCondition.Nm, record.Condition);
        Assert.Equal("Erste Pressung", record.Information);
        Assert.Equal(userId, record.UserId);
    }

    [Fact]
    public void Create_OhneArtist_ArtistIdIstNull()
    {
        // arrange
        var userId = Guid.NewGuid();

        // act
        var record = RecordEntity.Create(
            1, null, RecordFormat.Compilation, "Various Artists", 1999, RecordCondition.Vg, null, userId);

        // assert
        Assert.Null(record.ArtistId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_UngueltigeLabelId_WirftArgumentException(int labelId)
    {
        // arrange
        var userId = Guid.NewGuid();

        // act
        var act = () => RecordEntity.Create(
            labelId, null, RecordFormat.Album, "Abbey Road", 1969, RecordCondition.Vg, null, userId);

        // assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Create_LeererAlbumname_WirftArgumentException()
    {
        // arrange
        var userId = Guid.NewGuid();

        // act
        var act = () => RecordEntity.Create(
            1, null, RecordFormat.Album, string.Empty, 1969, RecordCondition.Vg, null, userId);

        // assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Create_AlbumnameZuLang_WirftArgumentException()
    {
        // arrange
        var userId = Guid.NewGuid();

        var zuLangerName = new string('a', RecordEntity.MaxAlbumNameLength + 1);

        // act
        var act = () => RecordEntity.Create(
            1, null, RecordFormat.Album, zuLangerName, 1969, RecordCondition.Vg, null, userId);

        // assert
        Assert.Throws<ArgumentException>(act);
    }

    [Theory]
    [InlineData("Abbey Road")]
    [InlineData("1989 (Taylor's Version)")]
    [InlineData("Ok Computer")]
    [InlineData("AC/DC Live")]
    [InlineData("Nevermind - Remastered")]
    public void Create_AlbumnameMitErlaubtenSonderzeichen_SetztEigenschaften(string albumName)
    {
        // arrange
        var userId = Guid.NewGuid();

        // act
        var record = RecordEntity.Create(
            1, null, RecordFormat.Album, albumName, 1969, RecordCondition.Vg, null, userId);

        // assert
        Assert.Equal(albumName, record.AlbumName);
    }

    [Theory]
    [InlineData("Ro@ck")]
    [InlineData("<script>")]
    [InlineData("Album!")]
    public void Create_AlbumnameMitVerbotenemSonderzeichen_WirftArgumentException(string albumName)
    {
        // arrange
        var userId = Guid.NewGuid();

        // act
        var act = () => RecordEntity.Create(
            1, null, RecordFormat.Album, albumName, 1969, RecordCondition.Vg, null, userId);

        // assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Create_ReleaseYearVorMindestjahr_WirftArgumentException()
    {
        // arrange
        var userId = Guid.NewGuid();

        // act
        var act = () => RecordEntity.Create(
            1,
            null,
            RecordFormat.Album,
            "Abbey Road",
            RecordEntity.MinReleaseYear - 1,
            RecordCondition.Vg,
            null,
            userId);

        // assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Create_ReleaseYearInDerZukunft_WirftArgumentException()
    {
        // arrange
        var userId = Guid.NewGuid();

        // act
        var act = () => RecordEntity.Create(
            1, null, RecordFormat.Album, "Abbey Road", _currentYear + 1, RecordCondition.Vg, null, userId);

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
        var record = RecordEntity.Create(
            1, null, RecordFormat.Album, "Abbey Road", 1969, RecordCondition.Vg, information, userId);

        // assert
        Assert.Equal(information, record.Information);
    }

    [Fact]
    public void Create_InformationZuLang_WirftArgumentException()
    {
        // arrange
        var userId = Guid.NewGuid();

        var zuLangeInformation = new string('a', RecordEntity.MaxInformationLength + 1);

        // act
        var act = () => RecordEntity.Create(
            1, null, RecordFormat.Album, "Abbey Road", 1969, RecordCondition.Vg, zuLangeInformation, userId);

        // assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Update_GibtNeueInstanzMitGeaendertenWertenZurueck_BehaeltIdUserIdUndCover()
    {
        // arrange
        var userId = Guid.NewGuid();

        var record = RecordEntity.Create(
            1, null, RecordFormat.Album, "Abbey Road", 1969, RecordCondition.Vg, null, userId);

        // act
        var updatedRecord = record.Update(
            2, 3, RecordFormat.CdAlbum, "Abbey Road (Remastered)", 2019, RecordCondition.Mint, "Jubiläumsausgabe");

        // assert
        Assert.NotSame(record, updatedRecord);
        Assert.Equal(2, updatedRecord.LabelId);
        Assert.Equal(3, updatedRecord.ArtistId);
        Assert.Equal(RecordFormat.CdAlbum, updatedRecord.Format);
        Assert.Equal("Abbey Road (Remastered)", updatedRecord.AlbumName);
        Assert.Equal(2019, updatedRecord.ReleaseYear);
        Assert.Equal(RecordCondition.Mint, updatedRecord.Condition);
        Assert.Equal("Jubiläumsausgabe", updatedRecord.Information);
        Assert.Equal(record.Id, updatedRecord.Id);
        Assert.Equal(userId, updatedRecord.UserId);
        Assert.Equal(record.AlbumCover, updatedRecord.AlbumCover);
    }
}
