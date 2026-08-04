namespace MyMusic.Application.Features.Stammdaten.Genre.Commands.Delete;

public sealed class DeleteGenreCommandHandler(
    IRepository<GenreEntity> repository,
    ICurrentUserService currentUserService,
    ExceptionManager exceptionManager)
    : ICommandHandler<DeleteGenreCommand, bool>
{
    public async Task<bool> HandleAsync(DeleteGenreCommand command, CancellationToken cancellationToken)
    {
        var genre = await repository.GetByIdAsync(command.Id, cancellationToken);

        if (genre is null || genre.UserId != currentUserService.UserId)
            throw exceptionManager.NotFound("Genre", command.Id);

        // Eine Referenzprüfung gegen record_track entfällt hier bewusst: Die Tabelle
        // entsteht erst mit Slice 6, bis dahin kann kein Track ein Genre referenzieren.
        repository.Remove(genre);

        await repository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
