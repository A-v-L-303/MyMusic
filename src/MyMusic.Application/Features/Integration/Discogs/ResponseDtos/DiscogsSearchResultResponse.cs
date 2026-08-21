namespace MyMusic.Application.Features.Integration.Discogs.ResponseDtos;

public sealed record DiscogsSearchResultResponse(int Id, string Title, int? Year, string? Label, string? ThumbnailUrl);
