namespace MyMusic.IntegrationTests;

public class SwaggerEndpointTests
{
    private static readonly TimeSpan _defaultTimeout = TimeSpan.FromMinutes(5);

    [Fact]
    public async Task GetSwaggerJson_InDevelopment200()
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

        // act
        var response = await apiClient.GetAsync("/swagger/v1/swagger.json", cancellationToken);

        // assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetSwaggerJson_AusserhalbDevelopmentNurMitAdminRolle()
    {
        // arrange
        var cancellationToken = CancellationToken.None;

        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.MyMusic_AppHost>(cancellationToken);

        var apiResource = appHost.Resources.OfType<ProjectResource>().Single(resource => resource.Name == "api");

        appHost.CreateResourceBuilder(apiResource).WithEnvironment("ASPNETCORE_ENVIRONMENT", "Production");

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

        // act: kein Token
        var unauthorizedResponse = await apiClient.GetAsync("/swagger/v1/swagger.json", cancellationToken);

        // assert
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorizedResponse.StatusCode);

        // arrange
        var admin = await KeycloakTestClient.CreateTestUserAsync(keycloakClient, appHost, cancellationToken);

        var plainUser = await KeycloakTestClient.CreateTestUserAsync(keycloakClient, appHost, cancellationToken);

        try
        {
            await KeycloakTestClient.AssignAdminRoleAsync(keycloakClient, appHost, admin, cancellationToken);

            var adminToken = await KeycloakTestClient.RequestUserAccessTokenAsync(
                keycloakClient, admin, cancellationToken);

            var plainUserToken = await KeycloakTestClient.RequestUserAccessTokenAsync(
                keycloakClient, plainUser, cancellationToken);

            // act: authentifiziert, aber ohne Admin-Rolle
            var forbiddenResponse = await GetSwaggerJsonAsync(apiClient, plainUserToken, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.Forbidden, forbiddenResponse.StatusCode);

            // act: mit Admin-Rolle
            var okResponse = await GetSwaggerJsonAsync(apiClient, adminToken, cancellationToken);

            // assert
            Assert.Equal(HttpStatusCode.OK, okResponse.StatusCode);
        }
        finally
        {
            await KeycloakTestClient.DeleteTestUserAsync(keycloakClient, appHost, admin, cancellationToken);

            await KeycloakTestClient.DeleteTestUserAsync(keycloakClient, appHost, plainUser, cancellationToken);
        }
    }

    private static async Task<HttpResponseMessage> GetSwaggerJsonAsync(
        HttpClient apiClient,
        string accessToken,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/swagger/v1/swagger.json");

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        return await apiClient.SendAsync(request, cancellationToken);
    }
}
