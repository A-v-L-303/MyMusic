namespace MyMusic.IntegrationTests;

public class CorsPolicyTests
{
    private static readonly TimeSpan _defaultTimeout = TimeSpan.FromMinutes(5);

    [Fact]
    public async Task PreflightRequest_FromLocalhostOrigin204MitGespiegeltemHeader()
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

        using var preflightRequest = new HttpRequestMessage(HttpMethod.Options, "/api/genres");

        preflightRequest.Headers.Add("Origin", "http://localhost:4200");
        preflightRequest.Headers.Add("Access-Control-Request-Method", "GET");
        preflightRequest.Headers.Add("Access-Control-Request-Headers", "authorization");

        // act
        var response = await apiClient.SendAsync(preflightRequest, cancellationToken);

        // assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal("http://localhost:4200", response.Headers.GetValues("Access-Control-Allow-Origin").Single());
    }

    [Fact]
    public async Task PreflightRequest_VonFremderOriginOhneAllowOriginHeader()
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

        using var preflightRequest = new HttpRequestMessage(HttpMethod.Options, "/api/genres");

        preflightRequest.Headers.Add("Origin", "https://example.com");
        preflightRequest.Headers.Add("Access-Control-Request-Method", "GET");
        preflightRequest.Headers.Add("Access-Control-Request-Headers", "authorization");

        // act
        var response = await apiClient.SendAsync(preflightRequest, cancellationToken);

        // assert: keine Freigabe für eine nicht-lokale Origin, unabhängig vom Statuscode der Antwort
        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
    }

    [Fact]
    public async Task PreflightRequest_AusserhalbDevelopmentNurVonWhitelisteterOrigin()
    {
        // arrange
        var cancellationToken = CancellationToken.None;

        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.MyMusic_AppHost>(cancellationToken);

        var apiResource = appHost.Resources.OfType<ProjectResource>().Single(resource => resource.Name == "api");

        appHost.CreateResourceBuilder(apiResource)
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Production")
            .WithEnvironment("Cors__AllowedOrigins__0", "https://mymusic.example.com");

        await using var app = await appHost.BuildAsync(cancellationToken).WaitAsync(_defaultTimeout, cancellationToken);

        await app.StartAsync(cancellationToken).WaitAsync(_defaultTimeout, cancellationToken);

        await app.ResourceNotifications
            .WaitForResourceAsync("migrator", KnownResourceStates.Finished, cancellationToken)
            .WaitAsync(_defaultTimeout, cancellationToken);

        await app.ResourceNotifications
            .WaitForResourceAsync("api", KnownResourceStates.Running, cancellationToken)
            .WaitAsync(_defaultTimeout, cancellationToken);

        using var apiClient = app.CreateHttpClient("api", "http");

        // act: whitelisted Origin
        var whitelistedResponse = await SendPreflightAsync(apiClient, "https://mymusic.example.com", cancellationToken);

        // assert
        Assert.Equal(HttpStatusCode.NoContent, whitelistedResponse.StatusCode);
        Assert.Equal(
            "https://mymusic.example.com",
            whitelistedResponse.Headers.GetValues("Access-Control-Allow-Origin").Single());

        // act: nicht gelistete Origin
        var notListedResponse = await SendPreflightAsync(apiClient, "https://example.com", cancellationToken);

        // assert: keine Freigabe für eine nicht gelistete Origin, unabhängig vom Statuscode der Antwort
        Assert.False(notListedResponse.Headers.Contains("Access-Control-Allow-Origin"));
    }

    private static async Task<HttpResponseMessage> SendPreflightAsync(
        HttpClient apiClient,
        string origin,
        CancellationToken cancellationToken)
    {
        using var preflightRequest = new HttpRequestMessage(HttpMethod.Options, "/api/genres");

        preflightRequest.Headers.Add("Origin", origin);
        preflightRequest.Headers.Add("Access-Control-Request-Method", "GET");
        preflightRequest.Headers.Add("Access-Control-Request-Headers", "authorization");

        return await apiClient.SendAsync(preflightRequest, cancellationToken);
    }
}
