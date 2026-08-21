namespace MyMusic.Application.Common.Services;

public sealed record DiscogsSearchResult(int Id, string Title, int? Year, string? Label, string? ThumbnailUrl);
