namespace MyMusic.Api.Endpoints.Stammdaten.Country;

public static class CountryEndpoints
{
    public static IEndpointRouteBuilder MapCountryEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/countries").RequireAuthorization();

        group.MapGet(string.Empty, GetAllCountriesAsync);

        return endpoints;
    }

    /// <summary>
    /// Gibt die vollständige, alphabetisch sortierte Länderliste zurück.
    /// </summary>
    private static async Task<IEnumerable<CountryResponse>> GetAllCountriesAsync(
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        return await mediator.SendAsync(new GetAllCountriesQuery(), cancellationToken);
    }
}
