namespace MyMusic.Application.Features.Stammdaten.Artist.Queries.GetById;

public sealed class GetArtistByIdQueryHandler(
    IRepository<ArtistEntity> repository,
    ExceptionManager exceptionManager,
    ArtistResponseBuilder responseBuilder)
    : IQueryHandler<GetArtistByIdQuery, ArtistResponse>
{
    public async Task<ArtistResponse> HandleAsync(GetArtistByIdQuery query, CancellationToken cancellationToken)
    {
        var artist = await repository.GetByIdAsync(query.Id, cancellationToken);

        if (artist is null || artist.UserId != query.UserId)
            throw exceptionManager.NotFound("Artist", query.Id);

        return responseBuilder.Build(artist);
    }
}
