namespace MyMusic.Infrastructure.ExternalServices.Discogs;

public sealed record DiscogsSearchResultRepresentation(
    int Id,
    string? Type,
    string? Title,
    string? Year,
    List<string>? Label,
    string? Thumb);
