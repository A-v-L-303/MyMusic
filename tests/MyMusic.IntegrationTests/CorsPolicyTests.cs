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
    public async Task PreflightRequest_AusserhalbDevelopmentVonWhitelistedOrigin204MitGespiegeltemHeader()
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

        using var preflightRequest = new HttpRequestMessage(HttpMethod.Options, "/api/genres");

        preflightRequest.Headers.Add("Origin", "https://mymusic.example.com");
        preflightRequest.Headers.Add("Access-Control-Request-Method", "GET");
        preflightRequest.Headers.Add("Access-Control-Request-Headers", "authorization");

        // act
        var response = await apiClient.SendAsync(preflightRequest, cancellationToken);

        // assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(
            "https://mymusic.example.com",
            response.Headers.GetValues("Access-Control-Allow-Origin").Single());
    }

    [Fact]
    public async Task PreflightRequest_AusserhalbDevelopmentVonNichtGelisteterOriginOhneAllowOriginHeader()
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

        using var preflightRequest = new HttpRequestMessage(HttpMethod.Options, "/api/genres");

        preflightRequest.Headers.Add("Origin", "https://example.com");
        preflightRequest.Headers.Add("Access-Control-Request-Method", "GET");
        preflightRequest.Headers.Add("Access-Control-Request-Headers", "authorization");

        // act
        var response = await apiClient.SendAsync(preflightRequest, cancellationToken);

        // assert: keine Freigabe für eine nicht gelistete Origin, unabhängig vom Statuscode der Antwort
        Assert.False(response.Headers.Contains("Access-Control-Allow-Origin"));
    }
}
