namespace MyMusic.Infrastructure.ExternalServices.Discogs;

public sealed record DiscogsTrackRepresentation(
    string? Position,
    string? Title,
    string? Duration,
    List<DiscogsArtistRepresentation>? Artists);
