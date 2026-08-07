namespace MyMusic.IntegrationTests.TestSupport;

public sealed record LabelListResponseDto(
    IReadOnlyList<LabelResponseDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);
