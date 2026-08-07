namespace MyMusic.Domain.Tests.DomainModels.Stammdaten.Label;

public class LabelTests
{
    [Fact]
    public void Create_GueltigeWerte_SetztEigenschaften()
    {
        // arrange
        var userId = Guid.NewGuid();

        // act
        var label = LabelEntity.Create("Rough Trade", 1, "Unabhängiges Label", userId);

        // assert
        Assert.Equal("Rough Trade", label.Name);
        Assert.Equal(1, label.CountryId);
        Assert.Equal("Unabhängiges Label", label.Information);
        Assert.Equal(userId, label.UserId);
    }

    [Fact]
    public void Create_LeererName_WirftArgumentException()
    {
        // arrange
        var userId = Guid.NewGuid();

        // act
        var act = () => LabelEntity.Create(string.Empty, 1, null, userId);

        // assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Create_NameZuLang_WirftArgumentException()
    {
        // arrange
        var userId = Guid.NewGuid();

        var zuLangerName = new string('a', LabelEntity.MaxNameLength + 1);

        // act
        var act = () => LabelEntity.Create(zuLangerName, 1, null, userId);

        // assert
        Assert.Throws<ArgumentException>(act);
    }

    [Theory]
    [InlineData("A&M Records")]
    [InlineData("4 A.D.")]
    [InlineData("Rough Trade")]
    [InlineData("AC/DC Records")]
    [InlineData("Sub-Pop")]
    [InlineData("O'Malley's")]
    public void Create_NameMitErlaubtenSonderzeichen_SetztEigenschaften(string name)
    {
        // arrange
        var userId = Guid.NewGuid();

        // act
        var label = LabelEntity.Create(name, 1, null, userId);

        // assert
        Assert.Equal(name, label.Name);
    }

    [Theory]
    [InlineData("Ro@ck")]
    [InlineData("Pop!")]
    [InlineData("<script>")]
    [InlineData("Parlophone (UK)")]
    public void Create_NameMitVerbotenemSonderzeichen_WirftArgumentException(string name)
    {
        // arrange
        var userId = Guid.NewGuid();

        // act
        var act = () => LabelEntity.Create(name, 1, null, userId);

        // assert
        Assert.Throws<ArgumentException>(act);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_UngueltigeCountryId_WirftArgumentException(int countryId)
    {
        // arrange
        var userId = Guid.NewGuid();

        // act
        var act = () => LabelEntity.Create("Rough Trade", countryId, null, userId);

        // assert
        Assert.Throws<ArgumentException>(act);
    }

    [Fact]
    public void Create_InformationZuLang_WirftArgumentException()
    {
        // arrange
        var userId = Guid.NewGuid();

        var zuLangeInformation = new string('a', LabelEntity.MaxInformationLength + 1);

        // act
        var act = () => LabelEntity.Create("Rough Trade", 1, zuLangeInformation, userId);

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
        var label = LabelEntity.Create("Rough Trade", 1, information, userId);

        // assert
        Assert.Equal(information, label.Information);
    }

    [Fact]
    public void Update_GibtNeueInstanzMitGeaendertenWertenZurueck_BehaeltIdUndUserId()
    {
        // arrange
        var userId = Guid.NewGuid();

        var label = LabelEntity.Create("Rough Trade", 1, null, userId);

        // act
        var updatedLabel = label.Update("Sub Pop", 2, "Aktualisiert");

        // assert
        Assert.NotSame(label, updatedLabel);
        Assert.Equal("Sub Pop", updatedLabel.Name);
        Assert.Equal(2, updatedLabel.CountryId);
        Assert.Equal("Aktualisiert", updatedLabel.Information);
        Assert.Equal(label.Id, updatedLabel.Id);
        Assert.Equal(userId, updatedLabel.UserId);
    }
}
