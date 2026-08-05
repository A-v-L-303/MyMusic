namespace MyMusic.Application.Features.Stammdaten.Country.ResponseDtos.Builder;

public sealed class CountryResponseBuilder
{
    public CountryResponse Build(CountryEntity country)
    {
        return new CountryResponse(country.Id, country.Name, country.Code);
    }
}
