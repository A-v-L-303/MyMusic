namespace MyMusic.Infrastructure.Tests.ExternalServices.Discogs.TestDoubles;

public sealed class FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
    : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(responder(request));
    }
}
