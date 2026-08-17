namespace MyMusic.IntegrationTests.TestSupport;

public sealed record UserListResponseDto(
    IReadOnlyList<UserResponseDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);
