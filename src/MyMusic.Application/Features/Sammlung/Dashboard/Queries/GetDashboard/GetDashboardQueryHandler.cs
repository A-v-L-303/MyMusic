namespace MyMusic.Application.Features.Sammlung.Dashboard.Queries.GetDashboard;

public sealed class GetDashboardQueryHandler(
    IRepository<RecordEntity> recordRepository,
    IRepository<ArtistEntity> artistRepository,
    IRepository<LabelEntity> labelRepository,
    IRepository<GenreEntity> genreRepository,
    DashboardResponseBuilder responseBuilder)
    : IQueryHandler<GetDashboardQuery, DashboardResponse>
{
    public async Task<DashboardResponse> HandleAsync(GetDashboardQuery query, CancellationToken cancellationToken)
    {
        var records = await recordRepository.GetProjectedAsync(
            record => record.UserId == query.UserId,
            record => new RecordAggregationProjection(
                record.Id, record.LabelId, record.ArtistId, record.Format, record.ReleaseYear),
            cancellationToken);

        var (_, artistsTotal) = await artistRepository.GetPagedAsync(
            artist => artist.UserId == query.UserId,
            queryable => queryable.OrderBy(artist => artist.Id),
            page: 1,
            pageSize: 1,
            cancellationToken);

        var (_, labelsTotal) = await labelRepository.GetPagedAsync(
            label => label.UserId == query.UserId,
            queryable => queryable.OrderBy(label => label.Id),
            page: 1,
            pageSize: 1,
            cancellationToken);

        var (_, genresTotal) = await genreRepository.GetPagedAsync(
            genre => genre.UserId == query.UserId,
            queryable => queryable.OrderBy(genre => genre.Id),
            page: 1,
            pageSize: 1,
            cancellationToken);

        var artistNamesById = await ResolveArtistNamesAsync(
            query.UserId,
            records.Where(record => record.ArtistId is not null).Select(record => record.ArtistId!.Value),
            cancellationToken);

        var labelNamesById = await ResolveLabelNamesAsync(
            query.UserId,
            records.Select(record => record.LabelId),
            cancellationToken);

        return responseBuilder.Build(
            records, artistsTotal, labelsTotal, genresTotal, artistNamesById, labelNamesById);
    }

    private async Task<IReadOnlyDictionary<int, string>> ResolveArtistNamesAsync(
        Guid userId,
        IEnumerable<int> artistIds,
        CancellationToken cancellationToken)
    {
        var distinctIds = artistIds.Distinct().ToList();

        if (distinctIds.Count == 0)
            return new Dictionary<int, string>();

        var (artists, _) = await artistRepository.GetPagedAsync(
            artist => artist.UserId == userId && distinctIds.Contains(artist.Id),
            queryable => queryable.OrderBy(artist => artist.Id),
            page: 1,
            pageSize: distinctIds.Count,
            cancellationToken);

        return artists.ToDictionary(artist => artist.Id, artist => artist.Name);
    }

    private async Task<IReadOnlyDictionary<int, string>> ResolveLabelNamesAsync(
        Guid userId,
        IEnumerable<int> labelIds,
        CancellationToken cancellationToken)
    {
        var distinctIds = labelIds.Distinct().ToList();

        if (distinctIds.Count == 0)
            return new Dictionary<int, string>();

        var (labels, _) = await labelRepository.GetPagedAsync(
            label => label.UserId == userId && distinctIds.Contains(label.Id),
            queryable => queryable.OrderBy(label => label.Id),
            page: 1,
            pageSize: distinctIds.Count,
            cancellationToken);

        return labels.ToDictionary(label => label.Id, label => label.Name);
    }
}
