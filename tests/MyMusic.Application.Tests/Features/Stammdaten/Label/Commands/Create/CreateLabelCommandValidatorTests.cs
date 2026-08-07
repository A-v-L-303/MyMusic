namespace MyMusic.Application.Tests.Features.Stammdaten.Label.Commands.Create;

public class CreateLabelCommandValidatorTests
{
    [Fact]
    public async Task ValidateAsync_GueltigeWerteUndExistierendesLand_KeinFehler()
    {
        // arrange
        var validator = CreateValidator(countryExists: true);

        var command = new CreateLabelCommand { Name = "Rough Trade", CountryId = 1, UserId = Guid.NewGuid() };

        // act
        var result = await validator.ValidateAsync(command);

        // assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task ValidateAsync_LeererName_LiefertFehler()
    {
        // arrange
        var validator = CreateValidator(countryExists: true);

        var command = new CreateLabelCommand { Name = string.Empty, CountryId = 1, UserId = Guid.NewGuid() };

        // act
        var result = await validator.ValidateAsync(command);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateLabelCommand.Name));
    }

    [Fact]
    public async Task ValidateAsync_NameZuLang_LiefertFehler()
    {
        // arrange
        var validator = CreateValidator(countryExists: true);

        var command = new CreateLabelCommand
        {
            Name = new string('a', LabelEntity.MaxNameLength + 1),
            CountryId = 1,
            UserId = Guid.NewGuid()
        };

        // act
        var result = await validator.ValidateAsync(command);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateLabelCommand.Name));
    }

    [Theory]
    [InlineData("A&M Records")]
    [InlineData("4 A.D.")]
    [InlineData("AC/DC Records")]
    public async Task ValidateAsync_NameMitErlaubtenSonderzeichen_KeinFehler(string name)
    {
        // arrange
        var validator = CreateValidator(countryExists: true);

        var command = new CreateLabelCommand { Name = name, CountryId = 1, UserId = Guid.NewGuid() };

        // act
        var result = await validator.ValidateAsync(command);

        // assert
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("Ro@ck")]
    [InlineData("<script>")]
    [InlineData("Parlophone (UK)")]
    public async Task ValidateAsync_NameMitVerbotenemSonderzeichen_LiefertFehler(string name)
    {
        // arrange
        var validator = CreateValidator(countryExists: true);

        var command = new CreateLabelCommand { Name = name, CountryId = 1, UserId = Guid.NewGuid() };

        // act
        var result = await validator.ValidateAsync(command);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateLabelCommand.Name));
    }

    [Fact]
    public async Task ValidateAsync_NichtExistierendesLand_LiefertFehler()
    {
        // arrange
        var validator = CreateValidator(countryExists: false);

        var command = new CreateLabelCommand { Name = "Rough Trade", CountryId = 999, UserId = Guid.NewGuid() };

        // act
        var result = await validator.ValidateAsync(command);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateLabelCommand.CountryId));
    }

    [Fact]
    public async Task ValidateAsync_InformationZuLang_LiefertFehler()
    {
        // arrange
        var validator = CreateValidator(countryExists: true);

        var command = new CreateLabelCommand
        {
            Name = "Rough Trade",
            CountryId = 1,
            Information = new string('a', LabelEntity.MaxInformationLength + 1),
            UserId = Guid.NewGuid()
        };

        // act
        var result = await validator.ValidateAsync(command);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateLabelCommand.Information));
    }

    private static CreateLabelCommandValidator CreateValidator(bool countryExists)
    {
        var countryRepository = Substitute.For<IRepository<CountryEntity>>();

        countryRepository.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(countryExists ? CountryEntity.Create("Vereinigtes Königreich", "GB") : null);

        return new CreateLabelCommandValidator(countryRepository);
    }
}
