namespace MyMusic.Application.Features.Stammdaten.Artist.Queries.GetPaged;

public sealed class GetPagedArtistsQueryHandler(
    IRepository<ArtistEntity> repository,
    IRepository<RecordEntity> recordRepository,
    ArtistResponseBuilder responseBuilder)
    : IQueryHandler<GetPagedArtistsQuery, ArtistListResponse>
{
    public async Task<ArtistListResponse> HandleAsync(GetPagedArtistsQuery query, CancellationToken cancellationToken)
    {
        var artistIdsForLabel = await ResolveArtistIdsForLabelAsync(query.UserId, query.LabelId, cancellationToken);

        var (items, totalCount) = await repository.GetPagedAsync(
            artist => artist.UserId == query.UserId
                && (query.Name == null || artist.Name.ToLower().Contains(query.Name.ToLower()))
                && (artistIdsForLabel == null || artistIdsForLabel.Contains(artist.Id)),
            queryable => queryable.OrderBy(artist => artist.Name),
            query.Page,
            query.PageSize,
            cancellationToken);

        return responseBuilder.BuildPaged(items, totalCount, query.Page, query.PageSize);
    }

    private async Task<HashSet<int>?> ResolveArtistIdsForLabelAsync(
        Guid userId,
        int? labelId,
        CancellationToken cancellationToken)
    {
        if (labelId is null)
            return null;

        var (records, _) = await recordRepository.GetPagedAsync(
            record => record.UserId == userId && record.LabelId == labelId,
            queryable => queryable.OrderBy(record => record.Id),
            page: 1,
            pageSize: int.MaxValue,
            cancellationToken);

        return records
            .Where(record => record.ArtistId is not null)
            .Select(record => record.ArtistId!.Value)
            .ToHashSet();
    }
}
