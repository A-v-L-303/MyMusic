namespace MyMusic.IntegrationTests;

public class CountryEndpointsTests
{
    private static readonly TimeSpan _defaultTimeout = TimeSpan.FromMinutes(5);

    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web);

    [Fact]
    public async Task CountryEndpoints_OhneTokenUnauthorized_MitTokenVollstaendigeSortierteListe()
    {
        // arrange
        var cancellationToken = CancellationToken.None;

        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.MyMusic_AppHost>(cancellationToken);

        await using var app = await appHost.BuildAsync(cancellationToken).WaitAsync(_defaultTimeout, cancellationToken);

        await app.StartAsync(cancellationToken).WaitAsync(_defaultTimeout, cancellationToken);

        await app.ResourceNotifications
            .WaitForResourceAsync("migrator", KnownResourceStates.Finished, cancellationToken)
            .WaitAsync(_defaultTimeout, cancellationToken);

        await app.ResourceNotifications
            .WaitForResourceAsync("api", KnownResourceStates.Running, cancellationToken)
            .WaitAsync(_defaultTimeout, cancellationToken);

        using var apiClient = app.CreateHttpClient("api", "http");
        using var keycloakClient = app.CreateHttpClient("keycloak", "http");

        // act
        var unauthorizedResponse = await apiClient.GetAsync("/api/countries", cancellationToken);

        // assert
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorizedResponse.StatusCode);

        // arrange
        var user = await KeycloakTestClient.CreateTestUserAsync(keycloakClient, appHost, cancellationToken);

        try
        {
            var accessToken = await KeycloakTestClient.RequestUserAccessTokenAsync(
                keycloakClient, user, cancellationToken);

            using var request = new HttpRequestMessage(HttpMethod.Get, "/api/countries");

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // act
            var response = await apiClient.SendAsync(request, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var countries = await response.Content
                .ReadFromJsonAsync<List<CountryResponseDto>>(_jsonOptions, cancellationToken);

            Assert.NotNull(countries);
            Assert.Equal(238, countries.Count);

            var sortedNames = countries.Select(country => country.Name).ToList();

            var expectedOrder = sortedNames.OrderBy(name => name, StringComparer.InvariantCulture).ToList();

            // Referenzliste ist alphabetisch nach Landesname sortiert (Klärung 2026-08-05).
            Assert.Equal(expectedOrder, sortedNames);

            Assert.Contains(countries, country => country.Name == "Deutschland" && country.Code == "DE");
        }
        finally
        {
            await KeycloakTestClient.DeleteTestUserAsync(keycloakClient, appHost, user, cancellationToken);
        }
    }
}
