namespace MyMusic.IntegrationTests.TestSupport;

public sealed record ArtistListResponseDto(
    IReadOnlyList<ArtistResponseDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);
