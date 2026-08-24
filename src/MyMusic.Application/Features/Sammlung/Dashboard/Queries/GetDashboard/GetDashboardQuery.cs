namespace MyMusic.Application.Features.Sammlung.Dashboard.Queries.GetDashboard;

public sealed record GetDashboardQuery(Guid UserId) : IQuery<DashboardResponse>;
