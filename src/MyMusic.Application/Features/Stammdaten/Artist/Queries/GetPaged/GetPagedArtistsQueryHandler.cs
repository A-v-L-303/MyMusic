namespace MyMusic.Application.Features.Stammdaten.Artist.Queries.GetPaged;

public sealed class GetPagedArtistsQueryHandler(
    IRepository<ArtistEntity> repository,
    ArtistResponseBuilder responseBuilder)
    : IQueryHandler<GetPagedArtistsQuery, ArtistListResponse>
{
    public async Task<ArtistListResponse> HandleAsync(GetPagedArtistsQuery query, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await repository.GetPagedAsync(
            artist => artist.UserId == query.UserId
                && (query.Name == null || artist.Name.ToLower().Contains(query.Name.ToLower())),
            queryable => queryable.OrderBy(artist => artist.Name),
            query.Page,
            query.PageSize,
            cancellationToken);

        return responseBuilder.BuildPaged(items, totalCount, query.Page, query.PageSize);
    }
}
