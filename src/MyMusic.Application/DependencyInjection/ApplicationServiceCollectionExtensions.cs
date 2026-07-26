namespace MyMusic.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var applicationAssembly = typeof(ApplicationServiceCollectionExtensions).Assembly;

        services.AddSingleton<ExceptionManager>();

        services.AddScoped<CurrentUserResponseBuilder>();

        services.AddScoped<CommandValidationDecorator>();

        services.AddScoped<IMediator, Mediator>();

        services.AddValidatorsFromAssembly(applicationAssembly);

        RegisterHandlers(services, applicationAssembly);

        return services;
    }

    private static void RegisterHandlers(IServiceCollection services, Assembly assembly)
    {
        var handlerInterfaceDefinitions = new[] { typeof(ICommandHandler<,>), typeof(IQueryHandler<,>) };

        var handlerTypes = assembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false });

        foreach (var handlerType in handlerTypes)
        {
            var implementedHandlerInterfaces = handlerType.GetInterfaces()
                .Where(implementedInterface => implementedInterface.IsGenericType
                    && handlerInterfaceDefinitions.Contains(implementedInterface.GetGenericTypeDefinition()));

            foreach (var implementedHandlerInterface in implementedHandlerInterfaces)
                services.AddScoped(implementedHandlerInterface, handlerType);
        }
    }
}
