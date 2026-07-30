namespace MyMusic.Application.Common.Exceptions.ExceptionManager;

public sealed class ExceptionManager
{
    public ValidationException ValidationFailed(IEnumerable<ValidationFailure> errors)
    {
        var groupedErrors = errors
            .GroupBy(error => error.PropertyName)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.ErrorMessage).ToArray());

        return new ValidationException(groupedErrors);
    }

    public NotFoundException NotFound<TId>(string entityName, TId id)
    {
        return new NotFoundException($"{entityName} mit der Id '{id}' wurde nicht gefunden.");
    }

    public ConflictException Conflict(string message)
    {
        return new ConflictException(message);
    }
}
