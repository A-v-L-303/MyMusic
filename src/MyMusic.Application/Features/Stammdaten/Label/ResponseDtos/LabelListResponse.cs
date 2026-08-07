namespace MyMusic.Application.Features.Stammdaten.Label.ResponseDtos;

public sealed record LabelListResponse(
    IReadOnlyList<LabelResponse> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);
