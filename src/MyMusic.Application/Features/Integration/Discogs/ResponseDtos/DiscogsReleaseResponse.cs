namespace MyMusic.Application.Features.Integration.Discogs.ResponseDtos;

public sealed record DiscogsReleaseResponse(
    int Id,
    string Title,
    int? Year,
    IReadOnlyList<string> Artists,
    IReadOnlyList<string> Labels,
    IReadOnlyList<string> Genres,
    IReadOnlyList<string> Styles,
    IReadOnlyList<DiscogsFormatResponse> Formats,
    string? CoverImageUrl,
    IReadOnlyList<DiscogsTrackResponse> Tracklist,
    string? Country);
