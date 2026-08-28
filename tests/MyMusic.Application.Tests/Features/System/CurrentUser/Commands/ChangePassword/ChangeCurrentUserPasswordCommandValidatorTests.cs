namespace MyMusic.Application.Tests.Features.System.CurrentUser.Commands.ChangePassword;

public class ChangeCurrentUserPasswordCommandValidatorTests
{
    private readonly ChangeCurrentUserPasswordCommandValidator _validator = new();

    [Fact]
    public void Validate_GueltigesPasswort_KeinFehler()
    {
        // arrange
        var command = new ChangeCurrentUserPasswordCommand
        {
            NewPassword = "einSicheresPasswort1",
            UserId = Guid.NewGuid()
        };

        // act
        var result = _validator.Validate(command);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_LeeresPasswort_LiefertFehler()
    {
        // arrange
        var command = new ChangeCurrentUserPasswordCommand { NewPassword = string.Empty, UserId = Guid.NewGuid() };

        // act
        var result = _validator.Validate(command);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors, error => error.PropertyName == nameof(ChangeCurrentUserPasswordCommand.NewPassword));
    }

    [Fact]
    public void Validate_PasswortZuKurz_LiefertFehler()
    {
        // arrange
        var command = new ChangeCurrentUserPasswordCommand { NewPassword = "kurz12", UserId = Guid.NewGuid() };

        // act
        var result = _validator.Validate(command);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors, error => error.PropertyName == nameof(ChangeCurrentUserPasswordCommand.NewPassword));
    }

    [Fact]
    public void Validate_PasswortZuLang_LiefertFehler()
    {
        // arrange
        var command = new ChangeCurrentUserPasswordCommand
        {
            NewPassword = new string('a', 101),
            UserId = Guid.NewGuid()
        };

        // act
        var result = _validator.Validate(command);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(
            result.Errors, error => error.PropertyName == nameof(ChangeCurrentUserPasswordCommand.NewPassword));
    }
}
