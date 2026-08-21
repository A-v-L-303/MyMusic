namespace MyMusic.Application.Common.Services;

public sealed record DiscogsRelease(
    int Id,
    string Title,
    int? Year,
    IReadOnlyList<string> Artists,
    IReadOnlyList<string> Labels,
    IReadOnlyList<string> Genres,
    IReadOnlyList<string> Styles,
    IReadOnlyList<DiscogsFormat> Formats,
    string? CoverImageUrl,
    IReadOnlyList<DiscogsTrack> Tracklist);
