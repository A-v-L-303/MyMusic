namespace MyMusic.Application.Features.Stammdaten.Artist.Commands.Delete;

public sealed class DeleteArtistCommandHandler(
    IRepository<ArtistEntity> repository,
    IRepository<RecordEntity> recordRepository,
    IRepository<RecordTrackEntity> recordTrackRepository,
    ICurrentUserService currentUserService,
    ExceptionManager exceptionManager)
    : ICommandHandler<DeleteArtistCommand, bool>
{
    public async Task<bool> HandleAsync(DeleteArtistCommand command, CancellationToken cancellationToken)
    {
        var artist = await repository.GetByIdAsync(command.Id, cancellationToken);

        if (artist is null || artist.UserId != currentUserService.UserId)
            throw exceptionManager.NotFound("Artist", command.Id);

        var (_, referencingRecordCount) = await recordRepository.GetPagedAsync(
            record => record.ArtistId == command.Id,
            query => query.OrderBy(record => record.Id),
            page: 1,
            pageSize: 1,
            cancellationToken);

        if (referencingRecordCount > 0)
            throw exceptionManager.Conflict(
                $"Artist '{artist.Name}' kann nicht gelöscht werden, da er noch von mindestens einem Record " +
                "verwendet wird.");

        var (_, referencingTrackCount) = await recordTrackRepository.GetPagedAsync(
            track => track.ArtistId == command.Id,
            query => query.OrderBy(track => track.Id),
            page: 1,
            pageSize: 1,
            cancellationToken);

        if (referencingTrackCount > 0)
            throw exceptionManager.Conflict(
                $"Artist '{artist.Name}' kann nicht gelöscht werden, da er noch von mindestens einem Track " +
                "verwendet wird.");

        repository.Remove(artist);

        await repository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
