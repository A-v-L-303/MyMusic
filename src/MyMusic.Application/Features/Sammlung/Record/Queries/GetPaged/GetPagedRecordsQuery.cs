namespace MyMusic.Application.Features.Sammlung.Record.Queries.GetPaged;

public sealed record GetPagedRecordsQuery(
    Guid UserId,
    int Page,
    int PageSize,
    string? Name,
    int? ArtistId,
    int? LabelId,
    int? YearFrom,
    int? YearTo,
    int? CountryId,
    string? SortBy,
    string? SortDirection) : IQuery<RecordListResponse>;
