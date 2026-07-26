namespace MyMusic.Application.Tests.DependencyInjection;

public class ApplicationServiceCollectionExtensionsTests
{
    [Fact]
    public async Task AddApplication_RegistriertMediatorUndPerAssemblyScanGefundeneHandler()
    {
        var services = new ServiceCollection();

        services.AddSingleton(Substitute.For<ICurrentUserService>());

        services.AddApplication();

        await using var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetService<IMediator>());
        Assert.NotNull(provider.GetService<IQueryHandler<GetCurrentUserQuery, CurrentUserResponse>>());
    }
}
