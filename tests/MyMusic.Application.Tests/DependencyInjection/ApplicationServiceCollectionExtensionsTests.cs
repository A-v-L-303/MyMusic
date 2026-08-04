namespace MyMusic.Application.Tests.DependencyInjection;

public class ApplicationServiceCollectionExtensionsTests
{
    [Fact]
    public async Task AddApplication_RegistriertMediatorUndPerAssemblyScanGefundeneHandler()
    {
        // arrange
        var services = new ServiceCollection();

        services.AddSingleton(Substitute.For<ICurrentUserService>());

        // act
        services.AddApplication();

        // assert
        await using var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetService<IMediator>());
        Assert.NotNull(provider.GetService<IQueryHandler<GetCurrentUserQuery, CurrentUserResponse>>());
    }
}
