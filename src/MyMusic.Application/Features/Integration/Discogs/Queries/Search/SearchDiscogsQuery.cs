namespace MyMusic.Application.Features.Integration.Discogs.Queries.Search;

public sealed record SearchDiscogsQuery(string Q) : IQuery<IEnumerable<DiscogsSearchResultResponse>>;
