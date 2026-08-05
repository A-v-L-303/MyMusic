namespace MyMusic.Application.Features.Stammdaten.Country.Queries.GetAll;

public sealed class GetAllCountriesQueryHandler(
    IRepository<CountryEntity> repository,
    CountryResponseBuilder responseBuilder)
    : IQueryHandler<GetAllCountriesQuery, IEnumerable<CountryResponse>>
{
    public async Task<IEnumerable<CountryResponse>> HandleAsync(
        GetAllCountriesQuery query,
        CancellationToken cancellationToken)
    {
        var countries = await repository.GetAllAsync(cancellationToken);

        return countries
            .OrderBy(country => country.Name, StringComparer.InvariantCulture)
            .Select(responseBuilder.Build);
    }
}
