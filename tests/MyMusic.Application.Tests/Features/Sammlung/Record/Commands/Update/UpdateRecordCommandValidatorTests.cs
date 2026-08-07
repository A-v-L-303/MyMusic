namespace MyMusic.Application.Tests.Features.Sammlung.Record.Commands.Update;

public class UpdateRecordCommandValidatorTests
{
    [Fact]
    public async Task ValidateAsync_GueltigeWerteMitEigenemLabelUndArtist_KeinFehler()
    {
        // arrange
        var userId = Guid.NewGuid();

        var validator = CreateValidator(labelOwnerId: userId, artistOwnerId: userId);

        var command = new UpdateRecordCommand
        {
            Id = 1,
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
    public async Task ValidateAsync_LeererAlbumname_LiefertFehler()
    {
        // arrange
        var userId = Guid.NewGuid();

        var validator = CreateValidator(labelOwnerId: userId, artistOwnerId: null);

        var command = new UpdateRecordCommand
        {
            Id = 1,
            LabelId = 1,
            AlbumName = string.Empty,
            ReleaseYear = 1969,
            UserId = userId
        };

        // act
        var result = await validator.ValidateAsync(command);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateRecordCommand.AlbumName));
    }

    [Fact]
    public async Task ValidateAsync_NichtExistierendesLabel_LiefertFehler()
    {
        // arrange
        var userId = Guid.NewGuid();

        var validator = CreateValidator(labelOwnerId: null, artistOwnerId: null);

        var command = new UpdateRecordCommand
        {
            Id = 1,
            LabelId = 999,
            AlbumName = "Abbey Road",
            ReleaseYear = 1969,
            UserId = userId
        };

        // act
        var result = await validator.ValidateAsync(command);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateRecordCommand.LabelId));
    }

    [Fact]
    public async Task ValidateAsync_LabelGehoertAnderemBenutzer_LiefertFehlerWieNichtExistent()
    {
        // arrange
        var userId = Guid.NewGuid();

        var validator = CreateValidator(labelOwnerId: Guid.NewGuid(), artistOwnerId: null);

        var command = new UpdateRecordCommand
        {
            Id = 1,
            LabelId = 1,
            AlbumName = "Abbey Road",
            ReleaseYear = 1969,
            UserId = userId
        };

        // act
        var result = await validator.ValidateAsync(command);

        // assert: Mandantentrennung - fremdes Label wird wie nicht existent behandelt (HTTP 400)
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateRecordCommand.LabelId));
    }

    [Fact]
    public async Task ValidateAsync_ArtistGehoertAnderemBenutzer_LiefertFehlerWieNichtExistent()
    {
        // arrange
        var userId = Guid.NewGuid();

        var validator = CreateValidator(labelOwnerId: userId, artistOwnerId: Guid.NewGuid());

        var command = new UpdateRecordCommand
        {
            Id = 1,
            LabelId = 1,
            ArtistId = 2,
            AlbumName = "Abbey Road",
            ReleaseYear = 1969,
            UserId = userId
        };

        // act
        var result = await validator.ValidateAsync(command);

        // assert: Mandantentrennung - fremder Artist wird wie nicht existent behandelt (HTTP 400)
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateRecordCommand.ArtistId));
    }

    [Fact]
    public async Task ValidateAsync_ReleaseYearAusserhalbBereich_LiefertFehler()
    {
        // arrange
        var userId = Guid.NewGuid();

        var validator = CreateValidator(labelOwnerId: userId, artistOwnerId: null);

        var command = new UpdateRecordCommand
        {
            Id = 1,
            LabelId = 1,
            AlbumName = "Abbey Road",
            ReleaseYear = RecordEntity.MinReleaseYear - 1,
            UserId = userId
        };

        // act
        var result = await validator.ValidateAsync(command);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateRecordCommand.ReleaseYear));
    }

    [Fact]
    public async Task ValidateAsync_InformationZuLang_LiefertFehler()
    {
        // arrange
        var userId = Guid.NewGuid();

        var validator = CreateValidator(labelOwnerId: userId, artistOwnerId: null);

        var command = new UpdateRecordCommand
        {
            Id = 1,
            LabelId = 1,
            AlbumName = "Abbey Road",
            ReleaseYear = 1969,
            Information = new string('a', RecordEntity.MaxInformationLength + 1),
            UserId = userId
        };

        // act
        var result = await validator.ValidateAsync(command);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateRecordCommand.Information));
    }

    private static UpdateRecordCommandValidator CreateValidator(Guid? labelOwnerId, Guid? artistOwnerId)
    {
        var labelRepository = Substitute.For<IRepository<LabelEntity>>();

        labelRepository.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(labelOwnerId is null ? null : LabelEntity.Create("Apple Records", 1, null, labelOwnerId.Value));

        var artistRepository = Substitute.For<IRepository<ArtistEntity>>();

        artistRepository.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(artistOwnerId is null ? null : ArtistEntity.Create("The Beatles", artistOwnerId.Value));

        return new UpdateRecordCommandValidator(labelRepository, artistRepository);
    }
}
