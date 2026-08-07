namespace MyMusic.Application.Features.Stammdaten.Label.Queries.GetPaged;

public sealed record GetPagedLabelsQuery(Guid UserId, int Page, int PageSize, string? Name, int? CountryId)
    : IQuery<LabelListResponse>;
