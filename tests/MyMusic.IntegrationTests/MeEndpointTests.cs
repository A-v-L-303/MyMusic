namespace MyMusic.IntegrationTests;

public class MeEndpointTests
{
    private static readonly TimeSpan _defaultTimeout = TimeSpan.FromMinutes(5);

    [Fact]
    public async Task GetMe_OhneToken401_MitGueltigemTokenDerEigenenUserId200()
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
        var unauthorizedResponse = await apiClient.GetAsync("/api/me", cancellationToken);

        // assert
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorizedResponse.StatusCode);

        // arrange
        var testUser = await KeycloakTestClient.CreateTestUserAsync(keycloakClient, appHost, cancellationToken);

        try
        {
            var accessToken = await KeycloakTestClient.RequestUserAccessTokenAsync(
                keycloakClient, testUser, cancellationToken);

            using var authorizedRequest = new HttpRequestMessage(HttpMethod.Get, "/api/me");

            authorizedRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            // act
            var authorizedResponse = await apiClient.SendAsync(authorizedRequest, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.OK, authorizedResponse.StatusCode);

            var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

            var body = await authorizedResponse.Content
                .ReadFromJsonAsync<CurrentUserResponseDto>(jsonOptions, cancellationToken);

            Assert.NotNull(body);
            Assert.Equal(testUser.Id, body.UserId.ToString());
        }
        finally
        {
            await KeycloakTestClient.DeleteTestUserAsync(keycloakClient, appHost, testUser, cancellationToken);
        }
    }
}
