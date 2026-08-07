namespace MyMusic.Application.Features.Sammlung.Record.Queries.GetPaged;

public sealed class GetPagedRecordsQueryHandler(
    IRepository<RecordEntity> repository,
    IRepository<LabelEntity> labelRepository,
    IRepository<ArtistEntity> artistRepository,
    RecordResponseBuilder responseBuilder)
    : IQueryHandler<GetPagedRecordsQuery, RecordListResponse>
{
    public async Task<RecordListResponse> HandleAsync(GetPagedRecordsQuery query, CancellationToken cancellationToken)
    {
        var labelIdsForCountry = await ResolveLabelIdsForCountryAsync(
            query.UserId,
            query.CountryId,
            cancellationToken);

        var (items, totalCount) = await repository.GetPagedAsync(
            record => record.UserId == query.UserId
                && (query.Name == null || record.AlbumName.ToLower().Contains(query.Name.ToLower()))
                && (query.ArtistId == null || record.ArtistId == query.ArtistId)
                && (query.LabelId == null || record.LabelId == query.LabelId)
                && (query.YearFrom == null || record.ReleaseYear >= query.YearFrom)
                && (query.YearTo == null || record.ReleaseYear <= query.YearTo)
                && (labelIdsForCountry == null || labelIdsForCountry.Contains(record.LabelId)),
            BuildOrderBy(query.SortBy, query.SortDirection),
            query.Page,
            query.PageSize,
            cancellationToken);

        var labelNamesById = await ResolveLabelNamesAsync(
            query.UserId,
            items.Select(record => record.LabelId),
            cancellationToken);

        var artistNamesById = await ResolveArtistNamesAsync(
            query.UserId,
            items.Where(record => record.ArtistId is not null).Select(record => record.ArtistId!.Value),
            cancellationToken);

        return responseBuilder.BuildPaged(
            items,
            labelNamesById,
            artistNamesById,
            totalCount,
            query.Page,
            query.PageSize);
    }

    private static Func<IQueryable<RecordEntity>, IOrderedQueryable<RecordEntity>> BuildOrderBy(
        string? sortBy,
        string? sortDirection)
    {
        var descending = string.Equals(sortDirection, "desc", StringComparison.OrdinalIgnoreCase);

        return sortBy?.ToLowerInvariant() switch
        {
            "releaseyear" => descending
                ? queryable => queryable.OrderByDescending(record => record.ReleaseYear)
                : queryable => queryable.OrderBy(record => record.ReleaseYear),
            "format" => descending
                ? queryable => queryable.OrderByDescending(record => record.Format)
                : queryable => queryable.OrderBy(record => record.Format),
            _ => descending
                ? queryable => queryable.OrderByDescending(record => record.AlbumName)
                : queryable => queryable.OrderBy(record => record.AlbumName)
        };
    }

    private async Task<HashSet<int>?> ResolveLabelIdsForCountryAsync(
        Guid userId,
        int? countryId,
        CancellationToken cancellationToken)
    {
        if (countryId is null)
            return null;

        var (labels, _) = await labelRepository.GetPagedAsync(
            label => label.UserId == userId && label.CountryId == countryId,
            queryable => queryable.OrderBy(label => label.Id),
            page: 1,
            pageSize: int.MaxValue,
            cancellationToken);

        return labels.Select(label => label.Id).ToHashSet();
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
}
