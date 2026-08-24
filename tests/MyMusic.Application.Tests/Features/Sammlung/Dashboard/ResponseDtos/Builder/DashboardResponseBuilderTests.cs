namespace MyMusic.Application.Tests.Features.Sammlung.Dashboard.ResponseDtos.Builder;

public class DashboardResponseBuilderTests
{
    [Fact]
    public void Build_LeereRecordsListe_GibtNullwerteUndLeereListenZurueck()
    {
        // arrange
        var builder = new DashboardResponseBuilder();

        // act
        var response = builder.Build(
            [], artistsTotal: 0, labelsTotal: 0, genresTotal: 0,
            artistNamesById: new Dictionary<int, string>(), labelNamesById: new Dictionary<int, string>());

        // assert
        Assert.Equal(0, response.RecordsTotal);
        Assert.Empty(response.FormatDistribution);
        Assert.Empty(response.TopArtists);
        Assert.Empty(response.TopLabels);
        Assert.Empty(response.YearDistribution);
    }

    [Fact]
    public void Build_MehrereFormate_SortiertFormatDistributionAbsteigendNachAnzahl()
    {
        // arrange
        var builder = new DashboardResponseBuilder();

        var records = new List<RecordAggregationProjection>
        {
            new(1, 1, null, RecordFormat.Album, 1990),
            new(2, 1, null, RecordFormat.CdAlbum, 1990),
            new(3, 1, null, RecordFormat.Album, 1991),
            new(4, 1, null, RecordFormat.Album, 1992),
        };

        // act
        var response = builder.Build(
            records, artistsTotal: 0, labelsTotal: 0, genresTotal: 0,
            artistNamesById: new Dictionary<int, string>(),
            labelNamesById: new Dictionary<int, string> { [1] = "Label" });

        // assert
        Assert.Equal(2, response.FormatDistribution.Count);
        Assert.Equal(RecordFormat.Album, response.FormatDistribution[0].Format);
        Assert.Equal(3, response.FormatDistribution[0].Count);
        Assert.Equal(RecordFormat.CdAlbum, response.FormatDistribution[1].Format);
        Assert.Equal(1, response.FormatDistribution[1].Count);
    }

    [Fact]
    public void Build_RecordsOhneArtistId_FliessenNichtInTopArtistsEin()
    {
        // arrange
        var builder = new DashboardResponseBuilder();

        var records = new List<RecordAggregationProjection>
        {
            new(1, 1, 5, RecordFormat.Album, 1990),
            new(2, 1, null, RecordFormat.Album, 1990),
            new(3, 1, null, RecordFormat.Album, 1990),
        };

        var artistNamesById = new Dictionary<int, string> { [5] = "Pink Floyd" };

        // act
        var response = builder.Build(
            records, artistsTotal: 0, labelsTotal: 0, genresTotal: 0,
            artistNamesById, labelNamesById: new Dictionary<int, string> { [1] = "Label" });

        // assert
        var topArtist = Assert.Single(response.TopArtists);
        Assert.Equal(5, topArtist.ArtistId);
        Assert.Equal("Pink Floyd", topArtist.ArtistName);
        Assert.Equal(1, topArtist.Count);
    }

    [Fact]
    public void Build_MehrAlsZehnArtists_BegrenztTopArtistsAufDieZehnMeistvertretenen()
    {
        // arrange
        var builder = new DashboardResponseBuilder();

        var records = new List<RecordAggregationProjection>();
        var artistNamesById = new Dictionary<int, string>();

        for (var artistId = 1; artistId <= 11; artistId++)
        {
            artistNamesById[artistId] = $"Artist {artistId}";

            var recordCount = artistId == 1 ? 3 : 1;

            for (var i = 0; i < recordCount; i++)
                records.Add(new RecordAggregationProjection(records.Count + 1, 1, artistId, RecordFormat.Album, 1990));
        }

        // act
        var response = builder.Build(
            records, artistsTotal: 0, labelsTotal: 0, genresTotal: 0,
            artistNamesById, labelNamesById: new Dictionary<int, string> { [1] = "Label" });

        // assert
        Assert.Equal(10, response.TopArtists.Count);
        Assert.Equal(1, response.TopArtists[0].ArtistId);
        Assert.Equal(3, response.TopArtists[0].Count);
    }

    [Fact]
    public void Build_MehrereJahre_SortiertJahresverteilungAufsteigendNachJahr()
    {
        // arrange
        var builder = new DashboardResponseBuilder();

        var records = new List<RecordAggregationProjection>
        {
            new(1, 1, null, RecordFormat.Album, 2000),
            new(2, 1, null, RecordFormat.Album, 1980),
            new(3, 1, null, RecordFormat.Album, 1990),
            new(4, 1, null, RecordFormat.Album, 1980),
        };

        // act
        var response = builder.Build(
            records, artistsTotal: 0, labelsTotal: 0, genresTotal: 0,
            artistNamesById: new Dictionary<int, string>(),
            labelNamesById: new Dictionary<int, string> { [1] = "Label" });

        // assert
        Assert.Equal([1980, 1990, 2000], response.YearDistribution.Select(entry => entry.Year));
        Assert.Equal(2, response.YearDistribution[0].Count);
    }

    [Fact]
    public void Build_TopLabels_SortiertAbsteigendNachAnzahlUndBegrenztAufZehn()
    {
        // arrange
        var builder = new DashboardResponseBuilder();

        var records = new List<RecordAggregationProjection>
        {
            new(1, 1, null, RecordFormat.Album, 1990),
            new(2, 1, null, RecordFormat.Album, 1990),
            new(3, 2, null, RecordFormat.Album, 1990),
        };

        var labelNamesById = new Dictionary<int, string> { [1] = "Label A", [2] = "Label B" };

        // act
        var response = builder.Build(
            records, artistsTotal: 0, labelsTotal: 0, genresTotal: 0,
            artistNamesById: new Dictionary<int, string>(), labelNamesById);

        // assert
        Assert.Equal(2, response.TopLabels.Count);
        Assert.Equal(1, response.TopLabels[0].LabelId);
        Assert.Equal("Label A", response.TopLabels[0].LabelName);
        Assert.Equal(2, response.TopLabels[0].Count);
    }

    [Fact]
    public void Build_RecordsTotal_EntsprichtDerAnzahlDerUebergebenenRecords()
    {
        // arrange
        var builder = new DashboardResponseBuilder();

        var records = new List<RecordAggregationProjection>
        {
            new(1, 1, null, RecordFormat.Album, 1990),
            new(2, 1, null, RecordFormat.Album, 1990),
        };

        // act
        var response = builder.Build(
            records, artistsTotal: 7, labelsTotal: 3, genresTotal: 9,
            artistNamesById: new Dictionary<int, string>(),
            labelNamesById: new Dictionary<int, string> { [1] = "Label" });

        // assert
        Assert.Equal(2, response.RecordsTotal);
        Assert.Equal(7, response.ArtistsTotal);
        Assert.Equal(3, response.LabelsTotal);
        Assert.Equal(9, response.GenresTotal);
    }
}
