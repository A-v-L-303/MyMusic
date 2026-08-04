namespace MyMusic.Domain.Tests.DomainModels.Stammdaten.Genre;

public class GenreTests
{
    [Fact]
    public void Create_GueltigerName_SetztEigenschaften()
    {
        // arrange
        var userId = Guid.NewGuid();

        // act
        var genre = GenreEntity.Create("Jazz", userId);

        // assert
        Assert.Equal("Jazz", genre.Name);
        Assert.Equal(userId, genre.UserId);
    }

    [Fact]
    public void Create_LeererName_WirftArgumentException()
    {
        // arrange
        var userId = Guid.NewGuid();

        // act
        var act = () => GenreEntity.Create(string.Empty, userId);

        // assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Create_NameZuLang_WirftArgumentException()
    {
        // arrange
        var userId = Guid.NewGuid();

        var zuLangerName = new string('a', GenreEntity.MaxNameLength + 1);

        // act
        var act = () => GenreEntity.Create(zuLangerName, userId);

        // assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Create_NameZuKurz_WirftArgumentException()
    {
        // arrange
        var userId = Guid.NewGuid();

        var zuKurzerName = new string('a', GenreEntity.MinNameLength - 1);

        // act
        var act = () => GenreEntity.Create(zuKurzerName, userId);

        // assert
        Assert.Throws<ArgumentException>(act);
    }

    [Theory]
    [InlineData("Hip-Hop")]
    [InlineData("R&B")]
    [InlineData("Rock'n'Roll")]
    [InlineData("K-Pop")]
    [InlineData("80s Pop")]
    public void Create_NameMitErlaubtenSonderzeichen_SetztEigenschaften(string name)
    {
        // arrange
        var userId = Guid.NewGuid();

        // act
        var genre = GenreEntity.Create(name, userId);

        // assert
        Assert.Equal(name, genre.Name);
    }

    [Theory]
    [InlineData("Ro@ck")]
    [InlineData("Pop!")]
    [InlineData("Jazz;")]
    [InlineData("<script>")]
    public void Create_NameMitVerbotenemSonderzeichen_WirftArgumentException(string name)
    {
        // arrange
        var userId = Guid.NewGuid();

        // act
        var act = () => GenreEntity.Create(name, userId);

        // assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Update_GibtNeueInstanzMitGeaendertemNamenZurueck_BehaeltIdUndUserId()
    {
        // arrange
        var userId = Guid.NewGuid();

        var genre = GenreEntity.Create("Rock", userId);

        // act
        var updatedGenre = genre.Update("Pop");

        // assert
        Assert.NotSame(genre, updatedGenre);
        Assert.Equal("Pop", updatedGenre.Name);
        Assert.Equal(genre.Id, updatedGenre.Id);
        Assert.Equal(userId, updatedGenre.UserId);
    }
}
