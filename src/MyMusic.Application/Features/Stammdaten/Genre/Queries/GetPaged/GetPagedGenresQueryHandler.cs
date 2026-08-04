namespace MyMusic.Application.Features.Stammdaten.Genre.Queries.GetPaged;

public sealed class GetPagedGenresQueryHandler(
    IRepository<GenreEntity> repository,
    GenreResponseBuilder responseBuilder)
    : IQueryHandler<GetPagedGenresQuery, GenreListResponse>
{
    public async Task<GenreListResponse> HandleAsync(GetPagedGenresQuery query, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await repository.GetPagedAsync(
            genre => genre.UserId == query.UserId
                && (query.Name == null || genre.Name.ToLower().Contains(query.Name.ToLower())),
            queryable => queryable.OrderBy(genre => genre.Name),
            query.Page,
            query.PageSize,
            cancellationToken);

        return responseBuilder.BuildPaged(items, totalCount, query.Page, query.PageSize);
    }
}
