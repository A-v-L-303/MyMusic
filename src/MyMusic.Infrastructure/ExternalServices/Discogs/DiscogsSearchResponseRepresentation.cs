namespace MyMusic.Infrastructure.ExternalServices.Discogs;

public sealed record DiscogsSearchResponseRepresentation(List<DiscogsSearchResultRepresentation>? Results);
