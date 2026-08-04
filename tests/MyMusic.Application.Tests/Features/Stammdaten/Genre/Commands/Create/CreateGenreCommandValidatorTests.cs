namespace MyMusic.Application.Tests.Features.Stammdaten.Genre.Commands.Create;

public class CreateGenreCommandValidatorTests
{
    private readonly CreateGenreCommandValidator _validator = new();

    [Fact]
    public void Validate_GueltigerName_KeinFehler()
    {
        // arrange
        var command = new CreateGenreCommand { Name = "Jazz", UserId = Guid.NewGuid() };

        // act
        var result = _validator.Validate(command);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_LeererName_LiefertFehler()
    {
        // arrange
        var command = new CreateGenreCommand { Name = string.Empty, UserId = Guid.NewGuid() };

        // act
        var result = _validator.Validate(command);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateGenreCommand.Name));
    }

    [Fact]
    public void Validate_NameZuLang_LiefertFehler()
    {
        // arrange
        var command = new CreateGenreCommand
        {
            Name = new string('a', GenreEntity.MaxNameLength + 1),
            UserId = Guid.NewGuid()
        };

        // act
        var result = _validator.Validate(command);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateGenreCommand.Name));
    }

    [Fact]
    public void Validate_NameZuKurz_LiefertFehler()
    {
        // arrange
        var command = new CreateGenreCommand
        {
            Name = new string('a', GenreEntity.MinNameLength - 1),
            UserId = Guid.NewGuid()
        };

        // act
        var result = _validator.Validate(command);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateGenreCommand.Name));
    }

    [Theory]
    [InlineData("Hip-Hop")]
    [InlineData("R&B")]
    [InlineData("Rock'n'Roll")]
    public void Validate_NameMitErlaubtenSonderzeichen_KeinFehler(string name)
    {
        // arrange
        var command = new CreateGenreCommand { Name = name, UserId = Guid.NewGuid() };

        // act
        var result = _validator.Validate(command);

        // assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("Ro@ck")]
    [InlineData("Pop!")]
    [InlineData("<script>")]
    public void Validate_NameMitVerbotenemSonderzeichen_LiefertFehler(string name)
    {
        // arrange
        var command = new CreateGenreCommand { Name = name, UserId = Guid.NewGuid() };

        // act
        var result = _validator.Validate(command);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateGenreCommand.Name));
    }
}
