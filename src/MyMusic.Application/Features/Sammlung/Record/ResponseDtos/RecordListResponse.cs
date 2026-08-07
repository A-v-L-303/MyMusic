namespace MyMusic.Application.Features.Sammlung.Record.ResponseDtos;

public sealed record RecordListResponse(
    IReadOnlyList<RecordResponse> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);
