namespace MyMusic.Application.Tests.Features.Stammdaten.Label.Commands.Update;

public class UpdateLabelCommandValidatorTests
{
    [Fact]
    public async Task ValidateAsync_GueltigeWerteUndExistierendesLand_KeinFehler()
    {
        // arrange
        var validator = CreateValidator(countryExists: true);

        var command = new UpdateLabelCommand
        {
            Id = 1,
            Name = "Sub Pop",
            CountryId = 1,
            UserId = Guid.NewGuid()
        };

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

        var command = new UpdateLabelCommand
        {
            Id = 1,
            Name = string.Empty,
            CountryId = 1,
            UserId = Guid.NewGuid()
        };

        // act
        var result = await validator.ValidateAsync(command);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateLabelCommand.Name));
    }

    [Fact]
    public async Task ValidateAsync_NichtExistierendesLand_LiefertFehler()
    {
        // arrange
        var validator = CreateValidator(countryExists: false);

        var command = new UpdateLabelCommand
        {
            Id = 1,
            Name = "Sub Pop",
            CountryId = 999,
            UserId = Guid.NewGuid()
        };

        // act
        var result = await validator.ValidateAsync(command);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateLabelCommand.CountryId));
    }

    [Fact]
    public async Task ValidateAsync_InformationZuLang_LiefertFehler()
    {
        // arrange
        var validator = CreateValidator(countryExists: true);

        var command = new UpdateLabelCommand
        {
            Id = 1,
            Name = "Sub Pop",
            CountryId = 1,
            Information = new string('a', LabelEntity.MaxInformationLength + 1),
            UserId = Guid.NewGuid()
        };

        // act
        var result = await validator.ValidateAsync(command);

        // assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateLabelCommand.Information));
    }

    private static UpdateLabelCommandValidator CreateValidator(bool countryExists)
    {
        var countryRepository = Substitute.For<IRepository<CountryEntity>>();

        countryRepository.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(countryExists ? CountryEntity.Create("Vereinigtes Königreich", "GB") : null);

        return new UpdateLabelCommandValidator(countryRepository);
    }
}
