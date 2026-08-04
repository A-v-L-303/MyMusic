namespace MyMusic.Application.Features.Stammdaten.Genre.Queries.GetById;

public sealed class GetGenreByIdQueryHandler(
    IRepository<GenreEntity> repository,
    ExceptionManager exceptionManager,
    GenreResponseBuilder responseBuilder)
    : IQueryHandler<GetGenreByIdQuery, GenreResponse>
{
    public async Task<GenreResponse> HandleAsync(GetGenreByIdQuery query, CancellationToken cancellationToken)
    {
        var genre = await repository.GetByIdAsync(query.Id, cancellationToken);

        if (genre is null || genre.UserId != query.UserId)
            throw exceptionManager.NotFound("Genre", query.Id);

        return responseBuilder.Build(genre);
    }
}
