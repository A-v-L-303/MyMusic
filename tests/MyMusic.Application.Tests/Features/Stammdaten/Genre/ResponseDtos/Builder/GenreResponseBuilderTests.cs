namespace MyMusic.Application.Tests.Features.Stammdaten.Genre.ResponseDtos.Builder;

public class GenreResponseBuilderTests
{
    private readonly GenreResponseBuilder _builder = new();

    [Fact]
    public void Build_MapptIdUndName()
    {
        // arrange
        var genre = GenreEntity.Create("Jazz", Guid.NewGuid());

        // act
        var response = _builder.Build(genre);

        // assert
        Assert.Equal(genre.Id, response.Id);
        Assert.Equal("Jazz", response.Name);
    }

    [Fact]
    public void BuildPaged_MapptItemsUndBerechnetGesamtseitenzahl()
    {
        // arrange
        var genres = new List<GenreEntity> { GenreEntity.Create("Jazz", Guid.NewGuid()) };

        // act
        var response = _builder.BuildPaged(genres, totalCount: 25, page: 2, pageSize: 10);

        // assert
        Assert.Single(response.Items);
        Assert.Equal("Jazz", response.Items[0].Name);
        Assert.Equal(25, response.TotalCount);
        Assert.Equal(2, response.Page);
        Assert.Equal(10, response.PageSize);
        Assert.Equal(3, response.TotalPages);
    }

    [Fact]
    public void BuildPaged_LeereListe_GibtLeereItemsZurueck()
    {
        // arrange
        var genres = new List<GenreEntity>();

        // act
        var response = _builder.BuildPaged(genres, totalCount: 0, page: 1, pageSize: 20);

        // assert
        Assert.Empty(response.Items);
        Assert.Equal(0, response.TotalPages);
    }
}
