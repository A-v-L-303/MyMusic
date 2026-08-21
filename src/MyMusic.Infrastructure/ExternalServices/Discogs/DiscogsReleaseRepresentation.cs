namespace MyMusic.Infrastructure.ExternalServices.Discogs;

public sealed record DiscogsReleaseRepresentation(
    int Id,
    string? Title,
    int? Year,
    List<DiscogsArtistRepresentation>? Artists,
    List<DiscogsLabelRepresentation>? Labels,
    List<string>? Genres,
    List<string>? Styles,
    List<DiscogsFormatRepresentation>? Formats,
    List<DiscogsImageRepresentation>? Images,
    List<DiscogsTrackRepresentation>? Tracklist);
