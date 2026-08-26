namespace MyMusic.IntegrationTests.TestSupport;

public sealed record SearchResultListResponseDto(
    IReadOnlyList<SearchResultResponseDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);
