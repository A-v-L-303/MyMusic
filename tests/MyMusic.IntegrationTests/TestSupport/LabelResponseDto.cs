namespace MyMusic.IntegrationTests.TestSupport;

public sealed record LabelResponseDto(int Id, string Name, int CountryId, string CountryName, string? Information);
