namespace MyMusic.Application.Features.Stammdaten.Artist.Queries.GetPaged;

public sealed record GetPagedArtistsQuery(Guid UserId, int Page, int PageSize, string? Name, int? LabelId)
    : IQuery<ArtistListResponse>;
