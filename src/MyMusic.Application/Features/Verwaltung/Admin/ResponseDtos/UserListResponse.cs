namespace MyMusic.Application.Features.Verwaltung.Admin.ResponseDtos;

public sealed record UserListResponse(
    IReadOnlyList<UserResponse> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);
