namespace MyMusic.Application.Features.Verwaltung.Admin.Commands.Delete;

public sealed class DeleteUserCommandHandler(
    IRepository<RecordEntity> recordRepository,
    IRepository<LabelEntity> labelRepository,
    IRepository<ArtistEntity> artistRepository,
    IRepository<GenreEntity> genreRepository,
    IKeycloakAdminClient keycloakAdminClient,
    ICurrentUserService currentUserService,
    ExceptionManager exceptionManager)
    : ICommandHandler<DeleteUserCommand, bool>
{
    public async Task<bool> HandleAsync(DeleteUserCommand command, CancellationToken cancellationToken)
    {
        if (command.TargetUserId == currentUserService.UserId)
            throw exceptionManager.Conflict("Der eigene Account kann nicht gelöscht werden.");

        var (records, _) = await recordRepository.GetPagedAsync(
            record => record.UserId == command.TargetUserId,
            queryable => queryable.OrderBy(record => record.Id),
            page: 1,
            pageSize: int.MaxValue,
            cancellationToken);

        foreach (var record in records)
            recordRepository.Remove(record);

        await recordRepository.SaveChangesAsync(cancellationToken);

        var (labels, _) = await labelRepository.GetPagedAsync(
            label => label.UserId == command.TargetUserId,
            queryable => queryable.OrderBy(label => label.Id),
            page: 1,
            pageSize: int.MaxValue,
            cancellationToken);

        foreach (var label in labels)
            labelRepository.Remove(label);

        await labelRepository.SaveChangesAsync(cancellationToken);

        var (artists, _) = await artistRepository.GetPagedAsync(
            artist => artist.UserId == command.TargetUserId,
            queryable => queryable.OrderBy(artist => artist.Id),
            page: 1,
            pageSize: int.MaxValue,
            cancellationToken);

        foreach (var artist in artists)
            artistRepository.Remove(artist);

        await artistRepository.SaveChangesAsync(cancellationToken);

        var (genres, _) = await genreRepository.GetPagedAsync(
            genre => genre.UserId == command.TargetUserId,
            queryable => queryable.OrderBy(genre => genre.Id),
            page: 1,
            pageSize: int.MaxValue,
            cancellationToken);

        foreach (var genre in genres)
            genreRepository.Remove(genre);

        await genreRepository.SaveChangesAsync(cancellationToken);

        await keycloakAdminClient.DeleteUserAsync(command.TargetUserId, cancellationToken);

        return true;
    }
}
