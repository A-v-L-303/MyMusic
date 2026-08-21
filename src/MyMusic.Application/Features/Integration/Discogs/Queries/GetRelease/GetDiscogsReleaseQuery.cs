namespace MyMusic.Application.Features.Integration.Discogs.Queries.GetRelease;

public sealed record GetDiscogsReleaseQuery(int Id) : IQuery<DiscogsReleaseResponse>;
