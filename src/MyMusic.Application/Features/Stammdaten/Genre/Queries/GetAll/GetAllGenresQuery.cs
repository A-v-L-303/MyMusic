namespace MyMusic.Application.Features.Stammdaten.Genre.Queries.GetAll;

public sealed record GetAllGenresQuery(Guid UserId) : IQuery<IEnumerable<GenreResponse>>;
