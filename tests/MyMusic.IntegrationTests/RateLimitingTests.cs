namespace MyMusic.IntegrationTests;

public class RateLimitingTests
{
    private static readonly TimeSpan _defaultTimeout = TimeSpan.FromMinutes(5);

    [Fact]
    public async Task GetMe_Nach100AnfragenProBenutzerInnerhalbEinerMinute429_AndererBenutzerWeiterhin200()
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

        var firstTestUser = await KeycloakTestClient.CreateTestUserAsync(keycloakClient, appHost, cancellationToken);
        var secondTestUser = await KeycloakTestClient.CreateTestUserAsync(keycloakClient, appHost, cancellationToken);

        try
        {
            var firstUserAccessToken = await KeycloakTestClient.RequestUserAccessTokenAsync(
                keycloakClient, firstTestUser, cancellationToken);

            // act: genau das Limit von 100 Anfragen ausschöpfen - alle müssen noch erfolgreich sein
            for (var requestNumber = 1; requestNumber <= 100; requestNumber++)
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, "/api/me");

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", firstUserAccessToken);

                var response = await apiClient.SendAsync(request, cancellationToken);

                // assert: Requests innerhalb des Limits duerfen nicht bereits abgelehnt werden
                Assert.True(
                    response.StatusCode == HttpStatusCode.OK,
                    $"Anfrage {requestNumber} von 100 wurde unerwartet mit {response.StatusCode} abgelehnt.");
            }

            using var rejectedRequest = new HttpRequestMessage(HttpMethod.Get, "/api/me");

            rejectedRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", firstUserAccessToken);

            // act: die 101. Anfrage desselben Benutzers innerhalb derselben Minute
            var rejectedResponse = await apiClient.SendAsync(rejectedRequest, cancellationToken);

            // assert
            Assert.Equal((HttpStatusCode)429, rejectedResponse.StatusCode);
            Assert.True(rejectedResponse.Headers.Contains("Retry-After"));

            var secondUserAccessToken = await KeycloakTestClient.RequestUserAccessTokenAsync(
                keycloakClient, secondTestUser, cancellationToken);

            using var secondUserRequest = new HttpRequestMessage(HttpMethod.Get, "/api/me");

            secondUserRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", secondUserAccessToken);

            // act: ein anderer Benutzer hat ein eigenes, noch unverbrauchtes Kontingent
            var secondUserResponse = await apiClient.SendAsync(secondUserRequest, cancellationToken);

            // assert: das Limit gilt pro Benutzer, nicht global über alle Benutzer hinweg
            Assert.Equal(HttpStatusCode.OK, secondUserResponse.StatusCode);
        }
        finally
        {
            await KeycloakTestClient.DeleteTestUserAsync(keycloakClient, appHost, firstTestUser, cancellationToken);
            await KeycloakTestClient.DeleteTestUserAsync(keycloakClient, appHost, secondTestUser, cancellationToken);
        }
    }

    [Fact]
    public async Task GetHealth_MehrAls100AnfragenOhneToken_BleibtImmerErreichbar()
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

        // act: deutlich mehr als 100 unautorisierte Anfragen auf einen Pfad ausserhalb von "/api" -
        // die Rate-Limiting-Middleware darf hier nicht greifen (siehe ADR 0022, Entscheidung 2)
        for (var requestNumber = 1; requestNumber <= 105; requestNumber++)
        {
            var response = await apiClient.GetAsync("/health", cancellationToken);

            // assert: /health liegt ausserhalb von "/api" und bleibt unabhaengig von der Anzahl erreichbar
            Assert.True(
                response.StatusCode == HttpStatusCode.OK,
                $"/health wurde bei Anfrage {requestNumber} von 105 unerwartet mit {response.StatusCode} beantwortet.");
        }
    }
}
