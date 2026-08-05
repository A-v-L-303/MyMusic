namespace MyMusic.Domain.Tests.DomainModels.Stammdaten.Country;

public class CountryTests
{
    [Fact]
    public void Create_GueltigerNameUndCode_SetztEigenschaften()
    {
        // arrange
        var name = "Deutschland";

        var code = "DE";

        // act
        var country = CountryEntity.Create(name, code);

        // assert
        Assert.Equal(name, country.Name);
        Assert.Equal(code, country.Code);
    }

    [Fact]
    public void Create_LeererName_WirftArgumentException()
    {
        // arrange
        var code = "DE";

        // act
        var act = () => CountryEntity.Create(string.Empty, code);

        // assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Create_NameZuLang_WirftArgumentException()
    {
        // arrange
        var zuLangerName = new string('a', CountryEntity.MaxNameLength + 1);

        var code = "DE";

        // act
        var act = () => CountryEntity.Create(zuLangerName, code);

        // assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Create_LeererCode_WirftArgumentException()
    {
        // arrange
        var name = "Deutschland";

        // act
        var act = () => CountryEntity.Create(name, string.Empty);

        // assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Create_CodeZuLang_WirftArgumentException()
    {
        // arrange
        var name = "Deutschland";

        var zuLangerCode = new string('a', CountryEntity.MaxCodeLength + 1);

        // act
        var act = () => CountryEntity.Create(name, zuLangerCode);

        // assert
        Assert.Throws<ArgumentException>(act);
    }

    [Theory]
    [InlineData("YU")]
    [InlineData("---")]
    public void Create_CodeOhneStandardZeichensatz_SetztEigenschaften(string code)
    {
        // arrange
        var name = "Jugoslawien";

        // act
        var country = CountryEntity.Create(name, code);

        // assert
        // Die Referenzliste enthält bewusst nicht-ISO-konforme Codes (historisch bzw. Platzhalter) —
        // es gibt keine Zeichensatzregel, die diese zu Unrecht ablehnen würde.
        Assert.Equal(code, country.Code);
    }
}
