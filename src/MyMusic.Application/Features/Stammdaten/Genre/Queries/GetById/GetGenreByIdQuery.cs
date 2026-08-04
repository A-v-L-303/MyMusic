namespace MyMusic.Application.Features.Stammdaten.Genre.Queries.GetById;

public sealed record GetGenreByIdQuery(int Id, Guid UserId) : IQuery<GenreResponse>;
