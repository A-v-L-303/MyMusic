namespace MyMusic.Application.Features.Stammdaten.Genre.Commands.Delete;

public sealed class DeleteGenreCommandHandler(
    IRepository<GenreEntity> repository,
    IRepository<RecordTrackEntity> recordTrackRepository,
    ICurrentUserService currentUserService,
    ExceptionManager exceptionManager)
    : ICommandHandler<DeleteGenreCommand, bool>
{
    public async Task<bool> HandleAsync(DeleteGenreCommand command, CancellationToken cancellationToken)
    {
        var genre = await repository.GetByIdAsync(command.Id, cancellationToken);

        if (genre is null || genre.UserId != currentUserService.UserId)
            throw exceptionManager.NotFound("Genre", command.Id);

        var (_, referencingTrackCount) = await recordTrackRepository.GetPagedAsync(
            track => track.GenreId == command.Id,
            query => query.OrderBy(track => track.Id),
            page: 1,
            pageSize: 1,
            cancellationToken);

        if (referencingTrackCount > 0)
            throw exceptionManager.Conflict(
                $"Genre '{genre.Name}' kann nicht gelöscht werden, da es noch von mindestens einem Track " +
                "verwendet wird.");

        repository.Remove(genre);

        await repository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
