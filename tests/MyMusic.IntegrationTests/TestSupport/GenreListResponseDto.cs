namespace MyMusic.IntegrationTests.TestSupport;

public sealed record GenreListResponseDto(
    IReadOnlyList<GenreResponseDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);
