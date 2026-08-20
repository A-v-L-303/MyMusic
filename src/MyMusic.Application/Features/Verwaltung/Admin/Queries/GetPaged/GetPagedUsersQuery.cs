namespace MyMusic.Application.Features.Verwaltung.Admin.Queries.GetPaged;

public sealed record GetPagedUsersQuery(int Page, int PageSize, string? Search) : IQuery<UserListResponse>;
