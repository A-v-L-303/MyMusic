namespace MyMusic.Infrastructure.Tests.ExternalServices.Discogs;

public class DiscogsClientTests
{
    private const string SearchResponseMitThumbnail =
        """
        { "results": [ { "id": 1, "type": "release", "title": "Nevermind", "year": "1991",
        "label": ["DGC"], "thumb": "https://img.discogs.com/thumb.jpg" } ] }
        """;

    private const string SearchResponseOhneThumbnail =
        """
        { "results": [ { "id": 1, "type": "release", "title": "Nevermind", "year": "1991",
        "label": ["DGC"], "thumb": null } ] }
        """;

    [Fact]
    public async Task SearchAsync_ThumbnailDownloadErfolgreich_LiefertThumbnailAlsDataUrl()
    {
        // arrange
        var handler = new FakeHttpMessageHandler(request => request.RequestUri!.AbsoluteUri switch
        {
            "https://api.discogs.com/database/search?q=nevermind&type=release" =>
                JsonResponse(SearchResponseMitThumbnail),
            "https://img.discogs.com/thumb.jpg" => ImageResponse([1, 2, 3], "image/jpeg"),
            _ => throw new InvalidOperationException($"Unerwarteter Request: {request.RequestUri}"),
        });

        var client = CreateClient(handler, out _);

        // act
        var results = await client.SearchAsync("nevermind", CancellationToken.None);

        // assert
        var result = Assert.Single(results);
        Assert.Equal("data:image/jpeg;base64,AQID", result.ThumbnailUrl);
    }

    [Fact]
    public async Task SearchAsync_ThumbnailDownloadSchlaegtFehl_LiefertNullThumbnailUndLoggtWarnung()
    {
        // arrange
        var handler = new FakeHttpMessageHandler(request => request.RequestUri!.AbsoluteUri switch
        {
            "https://api.discogs.com/database/search?q=nevermind&type=release" =>
                JsonResponse(SearchResponseMitThumbnail),
            "https://img.discogs.com/thumb.jpg" => new HttpResponseMessage(HttpStatusCode.NotFound),
            _ => throw new InvalidOperationException($"Unerwarteter Request: {request.RequestUri}"),
        });

        var client = CreateClient(handler, out var logger);

        // act
        var results = await client.SearchAsync("nevermind", CancellationToken.None);

        // assert: Suche schlaegt trotz fehlgeschlagenem Thumbnail-Download nicht insgesamt fehl
        var result = Assert.Single(results);
        Assert.Null(result.ThumbnailUrl);
        Assert.True(HatWarnungGeloggt(logger));
    }

    [Fact]
    public async Task SearchAsync_ErgebnisOhneThumbnailInDenRohdaten_LaedtKeinBildHerunter()
    {
        // arrange
        var handler = new FakeHttpMessageHandler(request => request.RequestUri!.AbsoluteUri switch
        {
            "https://api.discogs.com/database/search?q=nevermind&type=release" =>
                JsonResponse(SearchResponseOhneThumbnail),
            _ => throw new InvalidOperationException($"Unerwarteter Request: {request.RequestUri}"),
        });

        var client = CreateClient(handler, out _);

        // act
        var results = await client.SearchAsync("nevermind", CancellationToken.None);

        // assert: kein Aufruf auf ein Bild noetig, sonst haette FakeHttpMessageHandler geworfen
        var result = Assert.Single(results);
        Assert.Null(result.ThumbnailUrl);
    }

    [Fact]
    public async Task GetReleaseAsync_CoverDownloadErfolgreich_LiefertCoverAlsDataUrl()
    {
        // arrange
        var handler = new FakeHttpMessageHandler(request => request.RequestUri!.AbsoluteUri switch
        {
            "https://api.discogs.com/releases/1" => JsonResponse(ReleaseResponse("https://img.discogs.com/cover.jpg")),
            "https://img.discogs.com/cover.jpg" => ImageResponse([4, 5, 6], "image/png"),
            _ => throw new InvalidOperationException($"Unerwarteter Request: {request.RequestUri}"),
        });

        var client = CreateClient(handler, out _);

        // act
        var release = await client.GetReleaseAsync(1, CancellationToken.None);

        // assert
        Assert.Equal("data:image/png;base64,BAUG", release.CoverImageUrl);
    }

    [Fact]
    public async Task GetReleaseAsync_CoverDownloadSchlaegtFehl_LiefertReleaseOhneCoverUndLoggtWarnung()
    {
        // arrange
        var handler = new FakeHttpMessageHandler(request => request.RequestUri!.AbsoluteUri switch
        {
            "https://api.discogs.com/releases/1" => JsonResponse(ReleaseResponse("https://img.discogs.com/cover.jpg")),
            "https://img.discogs.com/cover.jpg" => new HttpResponseMessage(HttpStatusCode.NotFound),
            _ => throw new InvalidOperationException($"Unerwarteter Request: {request.RequestUri}"),
        });

        var client = CreateClient(handler, out var logger);

        // act
        var release = await client.GetReleaseAsync(1, CancellationToken.None);

        // assert: Release bleibt nutzbar, nur ohne Cover (ADR 0020)
        Assert.Null(release.CoverImageUrl);
        Assert.True(HatWarnungGeloggt(logger));
    }

    private static DiscogsClient CreateClient(FakeHttpMessageHandler handler, out ILogger<DiscogsClient> logger)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.discogs.com") };

        logger = Substitute.For<ILogger<DiscogsClient>>();

        return new DiscogsClient(httpClient, logger);
    }

    private static bool HatWarnungGeloggt(ILogger<DiscogsClient> logger)
    {
        return logger.ReceivedCalls().Any(call =>
            call.GetMethodInfo().Name == nameof(ILogger.Log)
            && call.GetArguments()[0] is LogLevel.Warning);
    }

    private static string ReleaseResponse(string coverImageUrl)
    {
        return $$"""
            { "id": 1, "title": "Nevermind", "year": 1991, "artists": [{ "name": "Nirvana" }],
            "labels": [{ "name": "DGC" }], "genres": [], "styles": [], "formats": [],
            "images": [{ "type": "primary", "uri": "{{coverImageUrl}}" }], "tracklist": [] }
            """;
    }

    private static HttpResponseMessage JsonResponse(string json)
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json"),
        };
    }

    private static HttpResponseMessage ImageResponse(byte[] bytes, string contentType)
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new ByteArrayContent(bytes)
            {
                Headers = { ContentType = new MediaTypeHeaderValue(contentType) },
            },
        };
    }
}
