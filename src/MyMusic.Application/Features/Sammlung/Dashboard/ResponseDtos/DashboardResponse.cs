namespace MyMusic.Application.Features.Sammlung.Dashboard.ResponseDtos;

public sealed record DashboardResponse(
    int RecordsTotal,
    int ArtistsTotal,
    int LabelsTotal,
    int GenresTotal,
    IReadOnlyList<FormatCountResponse> FormatDistribution,
    IReadOnlyList<TopArtistResponse> TopArtists,
    IReadOnlyList<TopLabelResponse> TopLabels,
    IReadOnlyList<YearCountResponse> YearDistribution);
