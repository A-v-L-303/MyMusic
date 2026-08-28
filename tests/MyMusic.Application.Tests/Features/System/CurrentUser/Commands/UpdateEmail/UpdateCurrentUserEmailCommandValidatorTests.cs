namespace MyMusic.Application.Tests.Features.System.CurrentUser.Commands.UpdateEmail;

public class UpdateCurrentUserEmailCommandValidatorTests
{
    private readonly UpdateCurrentUserEmailCommandValidator _validator = new();

    [Fact]
    public void Validate_GueltigeEmail_KeinFehler()
    {
        // arrange
        var command = new UpdateCurrentUserEmailCommand { Email = "neu@example.com", UserId = Guid.NewGuid() };

        // act
        var result = _validator.Validate(command);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_LeereEmail_LiefertFehler()
    {
        // arrange
        var command = new UpdateCurrentUserEmailCommand { Email = string.Empty, UserId = Guid.NewGuid() };

        // act
        var result = _validator.Validate(command);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateCurrentUserEmailCommand.Email));
    }

    [Fact]
    public void Validate_UngueltigesFormat_LiefertFehler()
    {
        // arrange
        var command = new UpdateCurrentUserEmailCommand { Email = "keine-email", UserId = Guid.NewGuid() };

        // act
        var result = _validator.Validate(command);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateCurrentUserEmailCommand.Email));
    }

    [Fact]
    public void Validate_EmailZuLang_LiefertFehler()
    {
        // arrange
        var command = new UpdateCurrentUserEmailCommand
        {
            Email = $"{new string('a', 115)}@example.com",
            UserId = Guid.NewGuid()
        };

        // act
        var result = _validator.Validate(command);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateCurrentUserEmailCommand.Email));
    }
}
