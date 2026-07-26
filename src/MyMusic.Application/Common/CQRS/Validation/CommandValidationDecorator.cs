namespace MyMusic.Application.Common.CQRS.Validation;

public sealed class CommandValidationDecorator(IServiceProvider serviceProvider, ExceptionManager exceptionManager)
{
    public async Task ValidateAsync(object command, CancellationToken cancellationToken)
    {
        var validatorType = typeof(IValidator<>).MakeGenericType(command.GetType());

        if (serviceProvider.GetService(validatorType) is not IValidator validator)
            return;

        var validationContext = new ValidationContext<object>(command);

        var validationResult = await validator.ValidateAsync(validationContext, cancellationToken);

        if (!validationResult.IsValid)
            throw exceptionManager.ValidationFailed(validationResult.Errors);
    }
}
