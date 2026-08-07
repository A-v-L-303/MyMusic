namespace MyMusic.Application.Tests.Features.Sammlung.Record.Commands.UploadCover;

public class UploadRecordCoverCommandValidatorTests
{
    private static readonly byte[] _jpegBytes = [0xFF, 0xD8, 0xFF, 0xE0];

    [Fact]
    public async Task ValidateAsync_GueltigesJpeg_KeinFehler()
    {
        // arrange
        var validator = new UploadRecordCoverCommandValidator();

        var command = new UploadRecordCoverCommand { Id = 1, UserId = Guid.NewGuid(), FileContent = _jpegBytes };

        // act
        var result = await validator.ValidateAsync(command);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ValidateAsync_LeereDatei_LiefertFehler()
    {
        // arrange
        var validator = new UploadRecordCoverCommandValidator();

        var command = new UploadRecordCoverCommand { Id = 1, UserId = Guid.NewGuid(), FileContent = [] };

        // act
        var result = await validator.ValidateAsync(command);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UploadRecordCoverCommand.FileContent));
    }

    [Fact]
    public async Task ValidateAsync_DateiZuGross_LiefertFehler()
    {
        // arrange
        var validator = new UploadRecordCoverCommandValidator();

        var zuGrosseDatei = new byte[RecordEntity.MaxAlbumCoverSizeBytes + 1];

        Array.Copy(_jpegBytes, zuGrosseDatei, _jpegBytes.Length);

        var command = new UploadRecordCoverCommand { Id = 1, UserId = Guid.NewGuid(), FileContent = zuGrosseDatei };

        // act
        var result = await validator.ValidateAsync(command);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UploadRecordCoverCommand.FileContent));
    }

    [Fact]
    public async Task ValidateAsync_UngueltigesFormat_LiefertFehler()
    {
        // arrange
        var validator = new UploadRecordCoverCommandValidator();

        var command = new UploadRecordCoverCommand
        {
            Id = 1,
            UserId = Guid.NewGuid(),
            FileContent = "Kein Bild"u8.ToArray()
        };

        // act
        var result = await validator.ValidateAsync(command);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UploadRecordCoverCommand.FileContent));
    }
}
