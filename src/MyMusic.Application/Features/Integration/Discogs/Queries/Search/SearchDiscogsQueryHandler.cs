namespace MyMusic.Application.Features.Integration.Discogs.Queries.Search;

public sealed class SearchDiscogsQueryHandler(
    IDiscogsClient discogsClient,
    DiscogsResponseBuilder responseBuilder,
    ExceptionManager exceptionManager)
    : IQueryHandler<SearchDiscogsQuery, IEnumerable<DiscogsSearchResultResponse>>
{
    private const int MinimumQueryLength = 2;

    public async Task<IEnumerable<DiscogsSearchResultResponse>> HandleAsync(
        SearchDiscogsQuery query,
        CancellationToken cancellationToken)
    {
        var trimmedQuery = query.Q.Trim();

        if (trimmedQuery.Length < MinimumQueryLength)
        {
            throw exceptionManager.ValidationFailed(
            [
                new ValidationFailure(
                    nameof(SearchDiscogsQuery.Q),
                    "Der Suchbegriff muss mindestens 2 Zeichen lang sein.")
            ]);
        }

        try
        {
            var results = await discogsClient.SearchAsync(trimmedQuery, cancellationToken);

            return results.Select(responseBuilder.BuildSearchResult);
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
