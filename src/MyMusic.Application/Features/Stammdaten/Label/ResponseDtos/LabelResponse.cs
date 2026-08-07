namespace MyMusic.Application.Features.Stammdaten.Label.ResponseDtos;

public sealed record LabelResponse(int Id, string Name, int CountryId, string CountryName, string? Information);
