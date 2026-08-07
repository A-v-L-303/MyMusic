namespace MyMusic.IntegrationTests.TestSupport;

public sealed record RecordListResponseDto(
    IReadOnlyList<RecordResponseDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);
