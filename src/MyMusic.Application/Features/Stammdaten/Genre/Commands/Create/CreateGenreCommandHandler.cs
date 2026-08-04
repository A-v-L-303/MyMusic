namespace MyMusic.Application.Features.Stammdaten.Genre.Commands.Create;

public sealed class CreateGenreCommandHandler(
    IRepository<GenreEntity> repository,
    ExceptionManager exceptionManager,
    GenreResponseBuilder responseBuilder)
    : ICommandHandler<CreateGenreCommand, GenreResponse>
{
    public async Task<GenreResponse> HandleAsync(CreateGenreCommand command, CancellationToken cancellationToken)
    {
        var (_, existingCount) = await repository.GetPagedAsync(
            genre => genre.UserId == command.UserId && genre.Name == command.Name,
            query => query.OrderBy(genre => genre.Id),
            page: 1,
            pageSize: 1,
            cancellationToken);

        if (existingCount > 0)
            throw exceptionManager.Conflict($"Ein Genre mit dem Namen '{command.Name}' existiert bereits.");

        var genre = GenreEntity.Create(command.Name, command.UserId);

        await repository.AddAsync(genre, cancellationToken);

        await repository.SaveChangesAsync(cancellationToken);

        return responseBuilder.Build(genre);
    }
}
