namespace MyMusic.Application.Tests.Features.Sammlung.RecordTrack.Commands.Update;

public class UpdateRecordTrackCommandValidatorTests
{
    [Fact]
    public async Task ValidateAsync_GueltigeWerteMitEigenemArtistUndGenre_KeinFehler()
    {
        // arrange
        var userId = Guid.NewGuid();

        var validator = CreateValidator(userId, artistOwnerId: userId, genreOwnerId: userId);

        var command = new UpdateRecordTrackCommand
        {
            Id = 1,
            RecordId = 1,
            ArtistId = 2,
            GenreId = 3,
            TrackName = "Come Together",
            RecordSide = "A",
            TrackNumber = 1,
            UserId = userId
        };

        // act
        var result = await validator.ValidateAsync(command);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ValidateAsync_ArtistGehoertAnderemBenutzer_LiefertFehlerWieNichtExistent()
    {
        // arrange
        var userId = Guid.NewGuid();

        var validator = CreateValidator(userId, artistOwnerId: Guid.NewGuid(), genreOwnerId: userId);

        var command = new UpdateRecordTrackCommand
        {
            Id = 1,
            RecordId = 1,
            ArtistId = 2,
            GenreId = 3,
            TrackName = "Come Together",
            RecordSide = "A",
            TrackNumber = 1,
            UserId = userId
        };

        // act
        var result = await validator.ValidateAsync(command);

        // assert: Mandantentrennung - fremder Artist wird wie nicht existent behandelt (HTTP 400)
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateRecordTrackCommand.ArtistId));
    }

    [Fact]
    public async Task ValidateAsync_GenreGehoertAnderemBenutzer_LiefertFehlerWieNichtExistent()
    {
        // arrange
        var userId = Guid.NewGuid();

        var validator = CreateValidator(userId, artistOwnerId: userId, genreOwnerId: Guid.NewGuid());

        var command = new UpdateRecordTrackCommand
        {
            Id = 1,
            RecordId = 1,
            ArtistId = 2,
            GenreId = 3,
            TrackName = "Come Together",
            RecordSide = "A",
            TrackNumber = 1,
            UserId = userId
        };

        // act
        var result = await validator.ValidateAsync(command);

        // assert: Mandantentrennung - fremdes Genre wird wie nicht existent behandelt (HTTP 400)
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateRecordTrackCommand.GenreId));
    }

    [Fact]
    public async Task ValidateAsync_LeererTrackname_LiefertFehler()
    {
        // arrange
        var userId = Guid.NewGuid();

        var validator = CreateValidator(userId, artistOwnerId: userId, genreOwnerId: userId);

        var command = new UpdateRecordTrackCommand
        {
            Id = 1,
            RecordId = 1,
            ArtistId = 2,
            GenreId = 3,
            TrackName = string.Empty,
            RecordSide = "A",
            TrackNumber = 1,
            UserId = userId
        };

        // act
        var result = await validator.ValidateAsync(command);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateRecordTrackCommand.TrackName));
    }

    [Fact]
    public async Task ValidateAsync_RecordSideMitSonderzeichen_LiefertFehler()
    {
        // arrange
        var userId = Guid.NewGuid();

        var validator = CreateValidator(userId, artistOwnerId: userId, genreOwnerId: userId);

        var command = new UpdateRecordTrackCommand
        {
            Id = 1,
            RecordId = 1,
            ArtistId = 2,
            GenreId = 3,
            TrackName = "Come Together",
            RecordSide = "A-",
            TrackNumber = 1,
            UserId = userId
        };

        // act
        var result = await validator.ValidateAsync(command);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateRecordTrackCommand.RecordSide));
    }

    [Fact]
    public async Task ValidateAsync_TrackNumberUnterMindestwert_LiefertFehler()
    {
        // arrange
        var userId = Guid.NewGuid();

        var validator = CreateValidator(userId, artistOwnerId: userId, genreOwnerId: userId);

        var command = new UpdateRecordTrackCommand
        {
            Id = 1,
            RecordId = 1,
            ArtistId = 2,
            GenreId = 3,
            TrackName = "Come Together",
            RecordSide = "A",
            TrackNumber = 0,
            UserId = userId
        };

        // act
        var result = await validator.ValidateAsync(command);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateRecordTrackCommand.TrackNumber));
    }

    [Fact]
    public async Task ValidateAsync_InformationZuLang_LiefertFehler()
    {
        // arrange
        var userId = Guid.NewGuid();

        var validator = CreateValidator(userId, artistOwnerId: userId, genreOwnerId: userId);

        var command = new UpdateRecordTrackCommand
        {
            Id = 1,
            RecordId = 1,
            ArtistId = 2,
            GenreId = 3,
            TrackName = "Come Together",
            RecordSide = "A",
            TrackNumber = 1,
            Information = new string('a', RecordTrackEntity.MaxInformationLength + 1),
            UserId = userId
        };

        // act
        var result = await validator.ValidateAsync(command);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateRecordTrackCommand.Information));
    }

    private static UpdateRecordTrackCommandValidator CreateValidator(
        Guid userId, Guid? artistOwnerId, Guid? genreOwnerId)
    {
        var artistRepository = Substitute.For<IRepository<ArtistEntity>>();

        artistRepository.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(artistOwnerId is null ? null : ArtistEntity.Create("The Beatles", artistOwnerId.Value));

        var genreRepository = Substitute.For<IRepository<GenreEntity>>();

        genreRepository.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(genreOwnerId is null ? null : GenreEntity.Create("Rock", genreOwnerId.Value));

        return new UpdateRecordTrackCommandValidator(artistRepository, genreRepository);
    }
}
