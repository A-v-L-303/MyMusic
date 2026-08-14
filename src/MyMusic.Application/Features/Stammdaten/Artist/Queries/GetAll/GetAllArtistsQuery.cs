namespace MyMusic.Application.Features.Stammdaten.Artist.Queries.GetAll;

public sealed record GetAllArtistsQuery(Guid UserId) : IQuery<IEnumerable<ArtistResponse>>;
