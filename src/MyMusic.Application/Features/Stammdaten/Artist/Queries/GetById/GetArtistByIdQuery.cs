namespace MyMusic.Application.Features.Stammdaten.Artist.Queries.GetById;

public sealed record GetArtistByIdQuery(int Id, Guid UserId) : IQuery<ArtistResponse>;
