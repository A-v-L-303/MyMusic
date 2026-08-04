namespace MyMusic.Application.Features.Stammdaten.Genre.Commands.Update;

public sealed class UpdateGenreCommandHandler(
    IRepository<GenreEntity> repository,
    ExceptionManager exceptionManager,
    GenreResponseBuilder responseBuilder)
    : ICommandHandler<UpdateGenreCommand, GenreResponse>
{
    public async Task<GenreResponse> HandleAsync(UpdateGenreCommand command, CancellationToken cancellationToken)
    {
        var existingGenre = await repository.GetByIdAsync(command.Id, cancellationToken);

        if (existingGenre is null || existingGenre.UserId != command.UserId)
            throw exceptionManager.NotFound("Genre", command.Id);

        var (_, conflictingCount) = await repository.GetPagedAsync(
            genre => genre.UserId == command.UserId && genre.Name == command.Name && genre.Id != command.Id,
            query => query.OrderBy(genre => genre.Id),
            page: 1,
            pageSize: 1,
            cancellationToken);

        if (conflictingCount > 0)
            throw exceptionManager.Conflict($"Ein Genre mit dem Namen '{command.Name}' existiert bereits.");

        var updatedGenre = existingGenre.Update(command.Name);

        repository.Update(updatedGenre);

        await repository.SaveChangesAsync(cancellationToken);

        return responseBuilder.Build(updatedGenre);
    }
}
