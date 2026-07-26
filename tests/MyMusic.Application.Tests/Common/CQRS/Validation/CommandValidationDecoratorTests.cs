namespace MyMusic.Application.Tests.Common.CQRS.Validation;

public class CommandValidationDecoratorTests
{
    [Fact]
    public async Task ValidateAsync_MitFehlschlagendemValidator_WirftValidationExceptionMitFehlern()
    {
        var services = new ServiceCollection();

        services.AddScoped<IValidator<SampleValidatedCommand>, SampleValidatedCommandValidator>();

        await using var provider = services.BuildServiceProvider();

        var decorator = new CommandValidationDecorator(provider, new ExceptionManager());

        var exception = await Assert.ThrowsAsync<ValidationException>(
            () => decorator.ValidateAsync(new SampleValidatedCommand(), CancellationToken.None));

        Assert.Contains("Name", exception.Errors.Keys);
    }

    [Fact]
    public async Task ValidateAsync_MitGueltigemCommand_WirftKeineException()
    {
        var services = new ServiceCollection();

        services.AddScoped<IValidator<SampleValidatedCommand>, SampleValidatedCommandValidator>();

        await using var provider = services.BuildServiceProvider();

        var decorator = new CommandValidationDecorator(provider, new ExceptionManager());

        await decorator.ValidateAsync(new SampleValidatedCommand { Name = "Vinyl" }, CancellationToken.None);
    }

    [Fact]
    public async Task ValidateAsync_OhneRegistriertenValidator_WirftKeineException()
    {
        var services = new ServiceCollection();

        await using var provider = services.BuildServiceProvider();

        var decorator = new CommandValidationDecorator(provider, new ExceptionManager());

        await decorator.ValidateAsync(new SampleValidatedCommand(), CancellationToken.None);
    }
}
