namespace MyMusic.Application.Features.Stammdaten.Artist.ResponseDtos.Builder;

public sealed class ArtistResponseBuilder
{
    public ArtistResponse Build(ArtistEntity artist)
    {
        return new ArtistResponse(artist.Id, artist.Name);
    }

    public ArtistListResponse BuildPaged(
        IReadOnlyList<ArtistEntity> artists,
        int totalCount,
        int page,
        int pageSize)
    {
        var items = artists.Select(Build).ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new ArtistListResponse(items, totalCount, page, pageSize, totalPages);
    }
}
