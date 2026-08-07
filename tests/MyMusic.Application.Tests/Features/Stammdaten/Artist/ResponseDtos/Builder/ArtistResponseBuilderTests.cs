namespace MyMusic.Application.Tests.Features.Stammdaten.Artist.ResponseDtos.Builder;

public class ArtistResponseBuilderTests
{
    private readonly ArtistResponseBuilder _builder = new();

    [Fact]
    public void Build_MapptIdUndName()
    {
        // arrange
        var artist = ArtistEntity.Create("Pink Floyd", Guid.NewGuid());

        // act
        var response = _builder.Build(artist);

        // assert
        Assert.Equal(artist.Id, response.Id);
        Assert.Equal("Pink Floyd", response.Name);
    }

    [Fact]
    public void BuildPaged_MapptItemsUndBerechnetGesamtseitenzahl()
    {
        // arrange
        var artists = new List<ArtistEntity> { ArtistEntity.Create("Pink Floyd", Guid.NewGuid()) };

        // act
        var response = _builder.BuildPaged(artists, totalCount: 25, page: 2, pageSize: 10);

        // assert
        Assert.Single(response.Items);
        Assert.Equal("Pink Floyd", response.Items[0].Name);
        Assert.Equal(25, response.TotalCount);
        Assert.Equal(2, response.Page);
        Assert.Equal(10, response.PageSize);
        Assert.Equal(3, response.TotalPages);
    }

    [Fact]
    public void BuildPaged_LeereListe_GibtLeereItemsZurueck()
    {
        // arrange
        var artists = new List<ArtistEntity>();

        // act
        var response = _builder.BuildPaged(artists, totalCount: 0, page: 1, pageSize: 20);

        // assert
        Assert.Empty(response.Items);
        Assert.Equal(0, response.TotalPages);
    }
}
