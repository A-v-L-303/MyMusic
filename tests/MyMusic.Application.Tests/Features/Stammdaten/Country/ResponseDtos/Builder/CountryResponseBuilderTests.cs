namespace MyMusic.Application.Tests.Features.Stammdaten.Country.ResponseDtos.Builder;

public class CountryResponseBuilderTests
{
    private readonly CountryResponseBuilder _builder = new();

    [Fact]
    public void Build_MapptIdNameUndCode()
    {
        // arrange
        var country = CountryEntity.Create("Deutschland", "DE");

        // act
        var response = _builder.Build(country);

        // assert
        Assert.Equal(country.Id, response.Id);
        Assert.Equal("Deutschland", response.Name);
        Assert.Equal("DE", response.Code);
    }
}
