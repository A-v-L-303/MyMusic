namespace MyMusic.Application.Features.Stammdaten.Genre.ResponseDtos.Builder;

public sealed class GenreResponseBuilder
{
    public GenreResponse Build(GenreEntity genre)
    {
        return new GenreResponse(genre.Id, genre.Name);
    }

    public GenreListResponse BuildPaged(IReadOnlyList<GenreEntity> genres, int totalCount, int page, int pageSize)
    {
        var items = genres.Select(Build).ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new GenreListResponse(items, totalCount, page, pageSize, totalPages);
    }
}
