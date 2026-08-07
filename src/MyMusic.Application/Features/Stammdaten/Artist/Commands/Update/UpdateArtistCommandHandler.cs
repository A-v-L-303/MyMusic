namespace MyMusic.Application.Features.Stammdaten.Artist.Commands.Update;

public sealed class UpdateArtistCommandHandler(
    IRepository<ArtistEntity> repository,
    ExceptionManager exceptionManager,
    ArtistResponseBuilder responseBuilder)
    : ICommandHandler<UpdateArtistCommand, ArtistResponse>
{
    public async Task<ArtistResponse> HandleAsync(UpdateArtistCommand command, CancellationToken cancellationToken)
    {
        var existingArtist = await repository.GetByIdAsync(command.Id, cancellationToken);

        if (existingArtist is null || existingArtist.UserId != command.UserId)
            throw exceptionManager.NotFound("Artist", command.Id);

        var (_, conflictingCount) = await repository.GetPagedAsync(
            artist => artist.UserId == command.UserId && artist.Name == command.Name && artist.Id != command.Id,
            query => query.OrderBy(artist => artist.Id),
            page: 1,
            pageSize: 1,
            cancellationToken);

        if (conflictingCount > 0)
            throw exceptionManager.Conflict($"Ein Artist mit dem Namen '{command.Name}' existiert bereits.");

        var updatedArtist = existingArtist.Update(command.Name);

        repository.Update(updatedArtist);

        await repository.SaveChangesAsync(cancellationToken);

        return responseBuilder.Build(updatedArtist);
    }
}
