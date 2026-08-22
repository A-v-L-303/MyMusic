namespace MyMusic.Application.Features.Integration.Discogs.ResponseDtos;

public sealed record DiscogsTrackResponse(string Position, string Title, string? Duration, string? Artist);
