namespace MyMusic.Application.Features.Sammlung.Dashboard.Queries.GetDashboard;

public sealed record RecordAggregationProjection(
    int Id,
    int LabelId,
    int? ArtistId,
    RecordFormat Format,
    int ReleaseYear);
