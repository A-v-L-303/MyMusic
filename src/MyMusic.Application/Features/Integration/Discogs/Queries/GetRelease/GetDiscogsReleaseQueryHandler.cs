namespace MyMusic.Application.Features.Integration.Discogs.Queries.GetRelease;

public sealed class GetDiscogsReleaseQueryHandler(
    IDiscogsClient discogsClient,
    DiscogsResponseBuilder responseBuilder,
    ExceptionManager exceptionManager)
    : IQueryHandler<GetDiscogsReleaseQuery, DiscogsReleaseResponse>
{
    public async Task<DiscogsReleaseResponse> HandleAsync(
        GetDiscogsReleaseQuery query,
        CancellationToken cancellationToken)
    {
        try
        {
            var release = await discogsClient.GetReleaseAsync(query.Id, cancellationToken);

            return responseBuilder.BuildRelease(release);
        }
        catch (Exception exception) when (IsDiscogsFailure(exception, cancellationToken))
        {
            throw exceptionManager.DiscogsUnavailable();
        }
    }

    private static bool IsDiscogsFailure(Exception exception, CancellationToken cancellationToken)
    {
        return exception is HttpRequestException or JsonException
            || (exception is TaskCanceledException && !cancellationToken.IsCancellationRequested);
    }
}
