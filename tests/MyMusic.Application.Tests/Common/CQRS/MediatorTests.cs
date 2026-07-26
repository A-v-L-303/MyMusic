namespace MyMusic.Application.Tests.Common.CQRS;

public class MediatorTests
{
    [Fact]
    public async Task SendAsync_Command_LoestKorrektenHandlerAufUndGibtErgebnisZurueck()
    {
        var services = new ServiceCollection();

        services.AddSingleton<ExceptionManager>();
        services.AddScoped<CommandValidationDecorator>();
        services.AddScoped<ICommandHandler<SampleCommand, string>, SampleCommandHandler>();
        services.AddScoped<IMediator, Mediator>();

        await using var provider = services.BuildServiceProvider();

        var mediator = provider.GetRequiredService<IMediator>();

        var result = await mediator.SendAsync(new SampleCommand(), CancellationToken.None);

        Assert.Equal("Command-Ergebnis", result);
    }

    [Fact]
    public async Task SendAsync_Query_LoestKorrektenHandlerAufUndGibtErgebnisZurueck()
    {
        var services = new ServiceCollection();

        services.AddSingleton<ExceptionManager>();
        services.AddScoped<CommandValidationDecorator>();
        services.AddScoped<IQueryHandler<SampleQuery, int>, SampleQueryHandler>();
        services.AddScoped<IMediator, Mediator>();

        await using var provider = services.BuildServiceProvider();

        var mediator = provider.GetRequiredService<IMediator>();

        var result = await mediator.SendAsync(new SampleQuery(), CancellationToken.None);

        Assert.Equal(42, result);
    }

    [Fact]
    public async Task SendAsync_Command_OhneRegistriertenHandler_WirftInvalidOperationException()
    {
        var services = new ServiceCollection();

        services.AddSingleton<ExceptionManager>();
        services.AddScoped<CommandValidationDecorator>();
        services.AddScoped<IMediator, Mediator>();

        await using var provider = services.BuildServiceProvider();

        var mediator = provider.GetRequiredService<IMediator>();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => mediator.SendAsync(new SampleCommand(), CancellationToken.None));
    }

    [Fact]
    public async Task SendAsync_Query_OhneRegistriertenHandler_WirftInvalidOperationException()
    {
        var services = new ServiceCollection();

        services.AddSingleton<ExceptionManager>();
        services.AddScoped<CommandValidationDecorator>();
        services.AddScoped<IMediator, Mediator>();

        await using var provider = services.BuildServiceProvider();

        var mediator = provider.GetRequiredService<IMediator>();

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => mediator.SendAsync(new SampleQuery(), CancellationToken.None));
    }
}
