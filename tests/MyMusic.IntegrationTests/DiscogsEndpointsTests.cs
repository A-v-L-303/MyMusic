namespace MyMusic.IntegrationTests;

public class DiscogsEndpointsTests
{
    private static readonly TimeSpan _defaultTimeout = TimeSpan.FromMinutes(5);

    [Fact]
    public async Task DiscogsEndpoints_OhneToken_LiefertUnauthorized()
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
        var searchResponse = await apiClient.GetAsync("/api/discogs/search?q=test", cancellationToken);

        var releaseResponse = await apiClient.GetAsync("/api/discogs/releases/1", cancellationToken);

        // assert: kein echter Discogs-Aufruf noetig, da beide Requests bereits an RequireAuthorization()
        // scheitern, bevor Handler oder IDiscogsClient erreicht werden
        Assert.Equal(HttpStatusCode.Unauthorized, searchResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, releaseResponse.StatusCode);
    }
}
