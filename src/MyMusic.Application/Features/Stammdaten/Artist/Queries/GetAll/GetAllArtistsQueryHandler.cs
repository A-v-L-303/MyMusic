namespace MyMusic.Application.Features.Stammdaten.Artist.Queries.GetAll;

public sealed class GetAllArtistsQueryHandler(
    IRepository<ArtistEntity> repository,
    ArtistResponseBuilder responseBuilder)
    : IQueryHandler<GetAllArtistsQuery, IEnumerable<ArtistResponse>>
{
    public async Task<IEnumerable<ArtistResponse>> HandleAsync(
        GetAllArtistsQuery query,
        CancellationToken cancellationToken)
    {
        var (items, _) = await repository.GetPagedAsync(
            artist => artist.UserId == query.UserId,
            queryable => queryable.OrderBy(artist => artist.Name),
            page: 1,
            pageSize: int.MaxValue,
            cancellationToken);

        return items.Select(responseBuilder.Build);
    }
}
