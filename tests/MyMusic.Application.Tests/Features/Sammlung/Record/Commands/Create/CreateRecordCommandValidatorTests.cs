namespace MyMusic.Application.Tests.Features.Sammlung.Record.Commands.Create;

public class CreateRecordCommandValidatorTests
{
    [Fact]
    public async Task ValidateAsync_GueltigeWerteMitEigenemLabelUndArtist_KeinFehler()
    {
        // arrange
        var userId = Guid.NewGuid();

        var validator = CreateValidator(userId, labelOwnerId: userId, artistOwnerId: userId);

        var command = new CreateRecordCommand
        {
            LabelId = 1,
            ArtistId = 2,
            Format = RecordFormat.Album,
            AlbumName = "Abbey Road",
            ReleaseYear = 1969,
            UserId = userId
        };

        // act
        var result = await validator.ValidateAsync(command);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ValidateAsync_OhneArtist_KeinFehler()
    {
        // arrange
        var userId = Guid.NewGuid();

        var validator = CreateValidator(userId, labelOwnerId: userId, artistOwnerId: null);

        var command = new CreateRecordCommand
        {
            LabelId = 1,
            ArtistId = null,
            Format = RecordFormat.Album,
            AlbumName = "Abbey Road",
            ReleaseYear = 1969,
            UserId = userId
        };

        // act
        var result = await validator.ValidateAsync(command);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ValidateAsync_NichtExistierendesLabel_LiefertFehler()
    {
        // arrange
        var userId = Guid.NewGuid();

        var validator = CreateValidator(userId, labelOwnerId: null, artistOwnerId: null);

        var command = new CreateRecordCommand
        {
            LabelId = 999,
            Format = RecordFormat.Album,
            AlbumName = "Abbey Road",
            ReleaseYear = 1969,
            UserId = userId
        };

        // act
        var result = await validator.ValidateAsync(command);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateRecordCommand.LabelId));
    }

    [Fact]
    public async Task ValidateAsync_LabelGehoertAnderemBenutzer_LiefertFehlerWieNichtExistent()
    {
        // arrange
        var userId = Guid.NewGuid();

        var validator = CreateValidator(userId, labelOwnerId: Guid.NewGuid(), artistOwnerId: null);

        var command = new CreateRecordCommand
        {
            LabelId = 1,
            Format = RecordFormat.Album,
            AlbumName = "Abbey Road",
            ReleaseYear = 1969,
            UserId = userId
        };

        // act
        var result = await validator.ValidateAsync(command);

        // assert: Mandantentrennung - fremdes Label wird wie nicht existent behandelt (HTTP 400)
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateRecordCommand.LabelId));
    }

    [Fact]
    public async Task ValidateAsync_NichtExistierenderArtist_LiefertFehler()
    {
        // arrange
        var userId = Guid.NewGuid();

        var validator = CreateValidator(userId, labelOwnerId: userId, artistOwnerId: null);

        var command = new CreateRecordCommand
        {
            LabelId = 1,
            ArtistId = 999,
            Format = RecordFormat.Album,
            AlbumName = "Abbey Road",
            ReleaseYear = 1969,
            UserId = userId
        };

        // act
        var result = await validator.ValidateAsync(command);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateRecordCommand.ArtistId));
    }

    [Fact]
    public async Task ValidateAsync_ArtistGehoertAnderemBenutzer_LiefertFehlerWieNichtExistent()
    {
        // arrange
        var userId = Guid.NewGuid();

        var validator = CreateValidator(userId, labelOwnerId: userId, artistOwnerId: Guid.NewGuid());

        var command = new CreateRecordCommand
        {
            LabelId = 1,
            ArtistId = 2,
            Format = RecordFormat.Album,
            AlbumName = "Abbey Road",
            ReleaseYear = 1969,
            UserId = userId
        };

        // act
        var result = await validator.ValidateAsync(command);

        // assert: Mandantentrennung - fremder Artist wird wie nicht existent behandelt (HTTP 400)
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateRecordCommand.ArtistId));
    }

    [Fact]
    public async Task ValidateAsync_LeererAlbumname_LiefertFehler()
    {
        // arrange
        var userId = Guid.NewGuid();

        var validator = CreateValidator(userId, labelOwnerId: userId, artistOwnerId: null);

        var command = new CreateRecordCommand
        {
            LabelId = 1,
            Format = RecordFormat.Album,
            AlbumName = string.Empty,
            ReleaseYear = 1969,
            UserId = userId
        };

        // act
        var result = await validator.ValidateAsync(command);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateRecordCommand.AlbumName));
    }

    [Theory]
    [InlineData("1989 (Taylor's Version)")]
    [InlineData("AC/DC Live")]
    public async Task ValidateAsync_AlbumnameMitErlaubtenSonderzeichenInklKlammern_KeinFehler(string albumName)
    {
        // arrange
        var userId = Guid.NewGuid();

        var validator = CreateValidator(userId, labelOwnerId: userId, artistOwnerId: null);

        var command = new CreateRecordCommand
        {
            LabelId = 1,
            Format = RecordFormat.Album,
            AlbumName = albumName,
            ReleaseYear = 1969,
            UserId = userId
        };

        // act
        var result = await validator.ValidateAsync(command);

        // assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("Album!")]
    [InlineData("<script>")]
    public async Task ValidateAsync_AlbumnameMitVerbotenemSonderzeichen_LiefertFehler(string albumName)
    {
        // arrange
        var userId = Guid.NewGuid();

        var validator = CreateValidator(userId, labelOwnerId: userId, artistOwnerId: null);

        var command = new CreateRecordCommand
        {
            LabelId = 1,
            Format = RecordFormat.Album,
            AlbumName = albumName,
            ReleaseYear = 1969,
            UserId = userId
        };

        // act
        var result = await validator.ValidateAsync(command);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateRecordCommand.AlbumName));
    }

    [Fact]
    public async Task ValidateAsync_ReleaseYearVorMindestjahr_LiefertFehler()
    {
        // arrange
        var userId = Guid.NewGuid();

        var validator = CreateValidator(userId, labelOwnerId: userId, artistOwnerId: null);

        var command = new CreateRecordCommand
        {
            LabelId = 1,
            Format = RecordFormat.Album,
            AlbumName = "Abbey Road",
            ReleaseYear = RecordEntity.MinReleaseYear - 1,
            UserId = userId
        };

        // act
        var result = await validator.ValidateAsync(command);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateRecordCommand.ReleaseYear));
    }

    [Fact]
    public async Task ValidateAsync_ReleaseYearInDerZukunft_LiefertFehler()
    {
        // arrange
        var userId = Guid.NewGuid();

        var validator = CreateValidator(userId, labelOwnerId: userId, artistOwnerId: null);

        var command = new CreateRecordCommand
        {
            LabelId = 1,
            Format = RecordFormat.Album,
            AlbumName = "Abbey Road",
            ReleaseYear = DateTime.UtcNow.Year + 1,
            UserId = userId
        };

        // act
        var result = await validator.ValidateAsync(command);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateRecordCommand.ReleaseYear));
    }

    [Fact]
    public async Task ValidateAsync_InformationZuLang_LiefertFehler()
    {
        // arrange
        var userId = Guid.NewGuid();

        var validator = CreateValidator(userId, labelOwnerId: userId, artistOwnerId: null);

        var command = new CreateRecordCommand
        {
            LabelId = 1,
            Format = RecordFormat.Album,
            AlbumName = "Abbey Road",
            ReleaseYear = 1969,
            Information = new string('a', RecordEntity.MaxInformationLength + 1),
            UserId = userId
        };

        // act
        var result = await validator.ValidateAsync(command);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateRecordCommand.Information));
    }

    private static CreateRecordCommandValidator CreateValidator(Guid userId, Guid? labelOwnerId, Guid? artistOwnerId)
    {
        var labelRepository = Substitute.For<IRepository<LabelEntity>>();

        labelRepository.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(labelOwnerId is null ? null : LabelEntity.Create("Apple Records", 1, null, labelOwnerId.Value));

        var artistRepository = Substitute.For<IRepository<ArtistEntity>>();

        artistRepository.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(artistOwnerId is null ? null : ArtistEntity.Create("The Beatles", artistOwnerId.Value));

        return new CreateRecordCommandValidator(labelRepository, artistRepository);
    }
}
