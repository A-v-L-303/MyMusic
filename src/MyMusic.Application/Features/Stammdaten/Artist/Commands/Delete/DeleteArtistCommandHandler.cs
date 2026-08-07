namespace MyMusic.Application.Features.Stammdaten.Artist.Commands.Delete;

public sealed class DeleteArtistCommandHandler(
    IRepository<ArtistEntity> repository,
    ICurrentUserService currentUserService,
    ExceptionManager exceptionManager)
    : ICommandHandler<DeleteArtistCommand, bool>
{
    public async Task<bool> HandleAsync(DeleteArtistCommand command, CancellationToken cancellationToken)
    {
        var artist = await repository.GetByIdAsync(command.Id, cancellationToken);

        if (artist is null || artist.UserId != currentUserService.UserId)
            throw exceptionManager.NotFound("Artist", command.Id);

        // Eine Referenzprüfung gegen record und record_track entfällt hier
        // bewusst: Die Tabellen entstehen erst mit Slice 6, bis dahin kann
        // kein Record und kein Track einen Artist referenzieren.
        repository.Remove(artist);

        await repository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
