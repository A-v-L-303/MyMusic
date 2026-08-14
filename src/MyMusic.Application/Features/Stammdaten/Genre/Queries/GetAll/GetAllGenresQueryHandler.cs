namespace MyMusic.Application.Features.Stammdaten.Genre.Queries.GetAll;

public sealed class GetAllGenresQueryHandler(IRepository<GenreEntity> repository, GenreResponseBuilder responseBuilder)
    : IQueryHandler<GetAllGenresQuery, IEnumerable<GenreResponse>>
{
    public async Task<IEnumerable<GenreResponse>> HandleAsync(
        GetAllGenresQuery query,
        CancellationToken cancellationToken)
    {
        var (items, _) = await repository.GetPagedAsync(
            genre => genre.UserId == query.UserId,
            queryable => queryable.OrderBy(genre => genre.Name),
            page: 1,
            pageSize: int.MaxValue,
            cancellationToken);

        return items.Select(responseBuilder.Build);
    }
}
