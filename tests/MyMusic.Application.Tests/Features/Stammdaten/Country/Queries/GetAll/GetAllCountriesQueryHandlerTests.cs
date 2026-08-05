namespace MyMusic.Application.Tests.Features.Stammdaten.Country.Queries.GetAll;

public class GetAllCountriesQueryHandlerTests
{
    [Fact]
    public async Task HandleAsync_UnsortierteEingabe_GibtAlphabetischSortierteListeZurueck()
    {
        // arrange
        var countries = new List<CountryEntity>
        {
            CountryEntity.Create("Zypern", "CY"),
            CountryEntity.Create("Albanien", "AL"),
            CountryEntity.Create("Deutschland", "DE"),
        };

        var repository = Substitute.For<IRepository<CountryEntity>>();

        repository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns((IEnumerable<CountryEntity>)countries);

        var handler = new GetAllCountriesQueryHandler(repository, new CountryResponseBuilder());

        // act
        var response = (await handler.HandleAsync(new GetAllCountriesQuery(), CancellationToken.None)).ToList();

        // assert
        Assert.Equal(3, response.Count);
        Assert.Equal("Albanien", response[0].Name);
        Assert.Equal("Deutschland", response[1].Name);
        Assert.Equal("Zypern", response[2].Name);
    }

    [Fact]
    public async Task HandleAsync_MapptIdNameUndCode()
    {
        // arrange
        var country = CountryEntity.Create("Deutschland", "DE");

        var repository = Substitute.For<IRepository<CountryEntity>>();

        repository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns((IEnumerable<CountryEntity>)new List<CountryEntity> { country });

        var handler = new GetAllCountriesQueryHandler(repository, new CountryResponseBuilder());

        // act
        var response = (await handler.HandleAsync(new GetAllCountriesQuery(), CancellationToken.None)).ToList();

        // assert
        Assert.Single(response);
        Assert.Equal(country.Id, response[0].Id);
        Assert.Equal("Deutschland", response[0].Name);
        Assert.Equal("DE", response[0].Code);
    }

    [Fact]
    public async Task HandleAsync_LeereListe_GibtLeereListeZurueck()
    {
        // arrange
        var repository = Substitute.For<IRepository<CountryEntity>>();

        repository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns((IEnumerable<CountryEntity>)new List<CountryEntity>());

        var handler = new GetAllCountriesQueryHandler(repository, new CountryResponseBuilder());

        // act
        var response = await handler.HandleAsync(new GetAllCountriesQuery(), CancellationToken.None);

        // assert
        Assert.Empty(response);
    }
}
