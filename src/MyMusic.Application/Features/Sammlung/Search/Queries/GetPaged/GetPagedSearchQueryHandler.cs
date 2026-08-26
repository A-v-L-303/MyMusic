namespace MyMusic.Application.Features.Sammlung.Search.Queries.GetPaged;

public sealed class GetPagedSearchQueryHandler(
    IRepository<RecordEntity> repository,
    IRepository<ArtistEntity> artistRepository,
    IRepository<LabelEntity> labelRepository,
    IRepository<GenreEntity> genreRepository,
    IRepository<CountryEntity> countryRepository,
    IRepository<RecordTrackEntity> recordTrackRepository,
    SearchResponseBuilder responseBuilder)
    : IQueryHandler<GetPagedSearchQuery, SearchResultListResponse>
{
    public async Task<SearchResultListResponse> HandleAsync(
        GetPagedSearchQuery query, CancellationToken cancellationToken)
    {
        var normalizedQuery = query.Query?.Trim().ToLower();

        if (string.IsNullOrEmpty(normalizedQuery))
            return responseBuilder.BuildPaged(
                [], new Dictionary<int, string>(), new Dictionary<int, string>(), 0, query.Page, query.PageSize);

        var matchingArtistIds = await ResolveMatchingArtistIdsAsync(query.UserId, normalizedQuery, cancellationToken);

        var matchingGenreIds = await ResolveMatchingGenreIdsAsync(query.UserId, normalizedQuery, cancellationToken);

        var matchingCountryIds = await ResolveMatchingCountryIdsAsync(normalizedQuery, cancellationToken);

        var matchingLabelIds = await ResolveMatchingLabelIdsAsync(
            query.UserId, normalizedQuery, matchingCountryIds, cancellationToken);

        var matchingRecordIdsViaTrack = await ResolveMatchingRecordIdsViaTrackAsync(
            query.UserId, matchingArtistIds, matchingGenreIds, cancellationToken);

        var (items, totalCount) = await repository.GetPagedAsync(
            record => record.UserId == query.UserId
                && (record.AlbumName.ToLower().Contains(normalizedQuery)
                    || (record.ArtistId != null && matchingArtistIds.Contains(record.ArtistId.Value))
                    || matchingLabelIds.Contains(record.LabelId)
                    || matchingRecordIdsViaTrack.Contains(record.Id)),
            queryable => queryable.OrderBy(record => record.AlbumName),
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
            items, labelNamesById, artistNamesById, totalCount, query.Page, query.PageSize);
    }

    private async Task<HashSet<int>> ResolveMatchingArtistIdsAsync(
        Guid userId, string normalizedQuery, CancellationToken cancellationToken)
    {
        var ids = await artistRepository.GetProjectedAsync(
            artist => artist.UserId == userId && artist.Name.ToLower().Contains(normalizedQuery),
            artist => artist.Id,
            cancellationToken);

        return ids.ToHashSet();
    }

    private async Task<HashSet<int>> ResolveMatchingGenreIdsAsync(
        Guid userId, string normalizedQuery, CancellationToken cancellationToken)
    {
        var ids = await genreRepository.GetProjectedAsync(
            genre => genre.UserId == userId && genre.Name.ToLower().Contains(normalizedQuery),
            genre => genre.Id,
            cancellationToken);

        return ids.ToHashSet();
    }

    private async Task<HashSet<int>> ResolveMatchingCountryIdsAsync(
        string normalizedQuery, CancellationToken cancellationToken)
    {
        var ids = await countryRepository.GetProjectedAsync(
            country => country.Name.ToLower().Contains(normalizedQuery),
            country => country.Id,
            cancellationToken);

        return ids.ToHashSet();
    }

    private async Task<HashSet<int>> ResolveMatchingLabelIdsAsync(
        Guid userId,
        string normalizedQuery,
        HashSet<int> matchingCountryIds,
        CancellationToken cancellationToken)
    {
        var ids = await labelRepository.GetProjectedAsync(
            label => label.UserId == userId
                && (label.Name.ToLower().Contains(normalizedQuery) || matchingCountryIds.Contains(label.CountryId)),
            label => label.Id,
            cancellationToken);

        return ids.ToHashSet();
    }

    private async Task<HashSet<int>> ResolveMatchingRecordIdsViaTrackAsync(
        Guid userId,
        HashSet<int> matchingArtistIds,
        HashSet<int> matchingGenreIds,
        CancellationToken cancellationToken)
    {
        if (matchingArtistIds.Count == 0 && matchingGenreIds.Count == 0)
            return [];

        var recordIds = await recordTrackRepository.GetProjectedAsync(
            track => track.UserId == userId
                && (matchingArtistIds.Contains(track.ArtistId) || matchingGenreIds.Contains(track.GenreId)),
            track => track.RecordId,
            cancellationToken);

        return recordIds.ToHashSet();
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
