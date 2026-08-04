namespace MyMusic.Application.Features.Stammdaten.Genre.Queries.GetPaged;

public sealed record GetPagedGenresQuery(Guid UserId, int Page, int PageSize, string? Name)
    : IQuery<GenreListResponse>;
