namespace MyMusic.Application.Tests.Features.Stammdaten.Artist.Commands.Create;

public class CreateArtistCommandValidatorTests
{
    private readonly CreateArtistCommandValidator _validator = new();

    [Fact]
    public void Validate_GueltigerName_KeinFehler()
    {
        // arrange
        var command = new CreateArtistCommand { Name = "Pink Floyd", UserId = Guid.NewGuid() };

        // act
        var result = _validator.Validate(command);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_LeererName_LiefertFehler()
    {
        // arrange
        var command = new CreateArtistCommand { Name = string.Empty, UserId = Guid.NewGuid() };

        // act
        var result = _validator.Validate(command);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateArtistCommand.Name));
    }

    [Fact]
    public void Validate_NameZuLang_LiefertFehler()
    {
        // arrange
        var command = new CreateArtistCommand
        {
            Name = new string('a', ArtistEntity.MaxNameLength + 1),
            UserId = Guid.NewGuid()
        };

        // act
        var result = _validator.Validate(command);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateArtistCommand.Name));
    }

    [Fact]
    public void Validate_NameZuKurz_LiefertFehler()
    {
        // arrange
        var command = new CreateArtistCommand
        {
            Name = new string('a', ArtistEntity.MinNameLength - 1),
            UserId = Guid.NewGuid()
        };

        // act
        var result = _validator.Validate(command);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateArtistCommand.Name));
    }

    [Theory]
    [InlineData("AC/DC")]
    [InlineData("Guns N' Roses")]
    [InlineData("R.E.M.")]
    public void Validate_NameMitErlaubtenSonderzeichen_KeinFehler(string name)
    {
        // arrange
        var command = new CreateArtistCommand { Name = name, UserId = Guid.NewGuid() };

        // act
        var result = _validator.Validate(command);

        // assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("Panic! at the Disco")]
    [InlineData("Crosby, Stills & Nash")]
    [InlineData("<script>")]
    public void Validate_NameMitVerbotenemSonderzeichen_LiefertFehler(string name)
    {
        // arrange
        var command = new CreateArtistCommand { Name = name, UserId = Guid.NewGuid() };

        // act
        var result = _validator.Validate(command);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateArtistCommand.Name));
    }
}
