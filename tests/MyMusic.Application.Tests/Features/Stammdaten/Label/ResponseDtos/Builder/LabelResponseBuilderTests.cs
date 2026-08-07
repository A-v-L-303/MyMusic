namespace MyMusic.Application.Tests.Features.Stammdaten.Label.ResponseDtos.Builder;

public class LabelResponseBuilderTests
{
    private readonly LabelResponseBuilder _builder = new();

    [Fact]
    public void Build_MapptAlleFelderInklusiveLandnamen()
    {
        // arrange
        var label = LabelEntity.Create("Rough Trade", 1, "Unabhängiges Label", Guid.NewGuid());

        // act
        var response = _builder.Build(label, "Vereinigtes Königreich");

        // assert
        Assert.Equal(label.Id, response.Id);
        Assert.Equal("Rough Trade", response.Name);
        Assert.Equal(1, response.CountryId);
        Assert.Equal("Vereinigtes Königreich", response.CountryName);
        Assert.Equal("Unabhängiges Label", response.Information);
    }

    [Fact]
    public void BuildPaged_LoestLandnamenJeItemUeberDictionaryAuf()
    {
        // arrange
        var labels = new List<LabelEntity>
        {
            LabelEntity.Create("Rough Trade", 1, null, Guid.NewGuid()),
            LabelEntity.Create("Sub Pop", 2, null, Guid.NewGuid())
        };

        var countryNamesById = new Dictionary<int, string>
        {
            [1] = "Vereinigtes Königreich",
            [2] = "USA"
        };

        // act
        var response = _builder.BuildPaged(labels, countryNamesById, totalCount: 25, page: 2, pageSize: 10);

        // assert
        Assert.Equal(2, response.Items.Count);
        Assert.Equal("Vereinigtes Königreich", response.Items[0].CountryName);
        Assert.Equal("USA", response.Items[1].CountryName);
        Assert.Equal(25, response.TotalCount);
        Assert.Equal(2, response.Page);
        Assert.Equal(10, response.PageSize);
        Assert.Equal(3, response.TotalPages);
    }

    [Fact]
    public void BuildPaged_LeereListe_GibtLeereItemsZurueck()
    {
        // arrange
        var labels = new List<LabelEntity>();

        var countryNamesById = new Dictionary<int, string>();

        // act
        var response = _builder.BuildPaged(labels, countryNamesById, totalCount: 0, page: 1, pageSize: 20);

        // assert
        Assert.Empty(response.Items);
        Assert.Equal(0, response.TotalPages);
    }
}
