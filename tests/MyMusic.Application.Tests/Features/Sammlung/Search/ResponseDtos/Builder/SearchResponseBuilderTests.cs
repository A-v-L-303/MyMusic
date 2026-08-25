namespace MyMusic.Application.Tests.Features.Sammlung.Search.ResponseDtos.Builder;

public class SearchResponseBuilderTests
{
    private readonly SearchResponseBuilder _builder = new();

    [Fact]
    public void Build_MapptAlleFelderInklusiveLabelUndArtistNamen()
    {
        // arrange
        var record = RecordEntity.Create(
            1, 2, RecordFormat.Album, "Abbey Road", 1969, RecordCondition.Nm, "Erste Pressung", Guid.NewGuid());

        // act
        var response = _builder.Build(record, "Apple Records", "The Beatles");

        // assert
        Assert.Equal(record.Id, response.Id);
        Assert.Equal(1, response.LabelId);
        Assert.Equal("Apple Records", response.LabelName);
        Assert.Equal(2, response.ArtistId);
        Assert.Equal("The Beatles", response.ArtistName);
        Assert.Equal(RecordFormat.Album, response.Format);
        Assert.Equal("Abbey Road", response.AlbumName);
        Assert.Equal(1969, response.ReleaseYear);
        Assert.Equal(RecordCondition.Nm, response.Condition);
        Assert.Equal("Erste Pressung", response.Information);
        Assert.Null(response.AlbumCoverDataUrl);
    }

    [Fact]
    public void Build_MitAlbumCover_LiefertDataUrlMitContentType()
    {
        // arrange
        var record = RecordEntity.Create(
            1, null, RecordFormat.Album, "Abbey Road", 1969, RecordCondition.Nm, null, Guid.NewGuid());

        byte[] pngBytes = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];

        var recordMitCover = record.SetAlbumCover(pngBytes);

        // act
        var response = _builder.Build(recordMitCover, "Apple Records", null);

        // assert
        Assert.Equal($"data:image/png;base64,{Convert.ToBase64String(pngBytes)}", response.AlbumCoverDataUrl);
    }

    [Fact]
    public void Build_OhneArtist_ArtistFelderSindNull()
    {
        // arrange
        var record = RecordEntity.Create(
            1, null, RecordFormat.Compilation, "Various Artists", 1999, RecordCondition.Vg, null, Guid.NewGuid());

        // act
        var response = _builder.Build(record, "Various Records", null);

        // assert
        Assert.Null(response.ArtistId);
        Assert.Null(response.ArtistName);
    }

    [Fact]
    public void BuildPaged_LoestLabelUndArtistNamenJeItemUeberDictionaryAuf()
    {
        // arrange
        var records = new List<RecordEntity>
        {
            RecordEntity.Create(
                1, 10, RecordFormat.Album, "Abbey Road", 1969, RecordCondition.Nm, null, Guid.NewGuid()),
            RecordEntity.Create(
                2, null, RecordFormat.Compilation, "Various", 1999, RecordCondition.Vg, null, Guid.NewGuid())
        };

        var labelNamesById = new Dictionary<int, string> { [1] = "Apple Records", [2] = "Various Records" };

        var artistNamesById = new Dictionary<int, string> { [10] = "The Beatles" };

        // act
        var response = _builder.BuildPaged(
            records, labelNamesById, artistNamesById, totalCount: 25, page: 2, pageSize: 10);

        // assert
        Assert.Equal(2, response.Items.Count);
        Assert.Equal("Apple Records", response.Items[0].LabelName);
        Assert.Equal("The Beatles", response.Items[0].ArtistName);
        Assert.Equal("Various Records", response.Items[1].LabelName);
        Assert.Null(response.Items[1].ArtistName);
        Assert.Equal(25, response.TotalCount);
        Assert.Equal(2, response.Page);
        Assert.Equal(10, response.PageSize);
        Assert.Equal(3, response.TotalPages);
    }

    [Fact]
    public void BuildPaged_LeereListe_GibtLeereItemsZurueck()
    {
        // arrange
        var records = new List<RecordEntity>();

        var labelNamesById = new Dictionary<int, string>();

        var artistNamesById = new Dictionary<int, string>();

        // act
        var response = _builder.BuildPaged(
            records, labelNamesById, artistNamesById, totalCount: 0, page: 1, pageSize: 20);

        // assert
        Assert.Empty(response.Items);
        Assert.Equal(0, response.TotalPages);
    }
}
