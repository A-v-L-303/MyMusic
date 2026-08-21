namespace MyMusic.Application.Features.Integration.Discogs.ResponseDtos;

public sealed record DiscogsFormatResponse(string Name, IReadOnlyList<string> Descriptions);
