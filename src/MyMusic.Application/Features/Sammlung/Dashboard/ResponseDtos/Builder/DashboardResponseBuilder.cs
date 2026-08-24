namespace MyMusic.Application.Features.Sammlung.Dashboard.ResponseDtos.Builder;

public sealed class DashboardResponseBuilder
{
    private const int TopCount = 10;

    public DashboardResponse Build(
        IReadOnlyList<RecordAggregationProjection> records,
        int artistsTotal,
        int labelsTotal,
        int genresTotal,
        IReadOnlyDictionary<int, string> artistNamesById,
        IReadOnlyDictionary<int, string> labelNamesById)
    {
        var formatDistribution = records
            .GroupBy(record => record.Format)
            .Select(group => new FormatCountResponse(group.Key, group.Count()))
            .OrderByDescending(entry => entry.Count)
            .ToList();

        var topArtists = records
            .Where(record => record.ArtistId is not null)
            .GroupBy(record => record.ArtistId!.Value)
            .Select(group => new TopArtistResponse(group.Key, artistNamesById[group.Key], group.Count()))
            .OrderByDescending(entry => entry.Count)
            .Take(TopCount)
            .ToList();

        var topLabels = records
            .GroupBy(record => record.LabelId)
            .Select(group => new TopLabelResponse(group.Key, labelNamesById[group.Key], group.Count()))
            .OrderByDescending(entry => entry.Count)
            .Take(TopCount)
            .ToList();

        var yearDistribution = records
            .GroupBy(record => record.ReleaseYear)
            .Select(group => new YearCountResponse(group.Key, group.Count()))
            .OrderBy(entry => entry.Year)
            .ToList();

        return new DashboardResponse(
            records.Count,
            artistsTotal,
            labelsTotal,
            genresTotal,
            formatDistribution,
            topArtists,
            topLabels,
            yearDistribution);
    }
}
