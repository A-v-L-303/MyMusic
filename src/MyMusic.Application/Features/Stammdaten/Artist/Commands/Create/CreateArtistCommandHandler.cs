namespace MyMusic.Application.Features.Stammdaten.Artist.Commands.Create;

public sealed class CreateArtistCommandHandler(
    IRepository<ArtistEntity> repository,
    ExceptionManager exceptionManager,
    ArtistResponseBuilder responseBuilder)
    : ICommandHandler<CreateArtistCommand, ArtistResponse>
{
    public async Task<ArtistResponse> HandleAsync(CreateArtistCommand command, CancellationToken cancellationToken)
    {
        var (_, existingCount) = await repository.GetPagedAsync(
            artist => artist.UserId == command.UserId && artist.Name == command.Name,
            query => query.OrderBy(artist => artist.Id),
            page: 1,
            pageSize: 1,
            cancellationToken);

        if (existingCount > 0)
            throw exceptionManager.Conflict($"Ein Artist mit dem Namen '{command.Name}' existiert bereits.");

        var artist = ArtistEntity.Create(command.Name, command.UserId);

        await repository.AddAsync(artist, cancellationToken);

        await repository.SaveChangesAsync(cancellationToken);

        return responseBuilder.Build(artist);
    }
}
