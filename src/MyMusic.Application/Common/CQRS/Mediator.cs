namespace MyMusic.Application.Common.CQRS;

public sealed class Mediator(IServiceProvider serviceProvider, CommandValidationDecorator validationDecorator)
    : IMediator
{
    public async Task<TResponse> SendAsync<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken)
    {
        await validationDecorator.ValidateAsync(command, cancellationToken);

        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(command.GetType(), typeof(TResponse));

        return await InvokeAsync<TResponse>(handlerType, command, cancellationToken);
    }

    public Task<TResponse> SendAsync<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken)
    {
        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResponse));

        return InvokeAsync<TResponse>(handlerType, query, cancellationToken);
    }

    private Task<TResponse> InvokeAsync<TResponse>(
        Type handlerType,
        object request,
        CancellationToken cancellationToken)
    {
        var handler = serviceProvider.GetService(handlerType);

        if (handler is null)
            throw new InvalidOperationException(
                $"Für den Typ '{request.GetType().Name}' ist kein Handler registriert.");

        var method = handlerType.GetMethod("HandleAsync")!;

        return (Task<TResponse>)method.Invoke(handler, [request, cancellationToken])!;
    }
}
