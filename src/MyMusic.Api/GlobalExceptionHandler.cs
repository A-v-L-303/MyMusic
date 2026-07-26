namespace MyMusic.Api;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, problemDetails) = MapException(exception);

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, problemDetails.GetType(), cancellationToken);

        return true;
    }

    private static (int StatusCode, ProblemDetails ProblemDetails) MapException(Exception exception)
    {
        return exception switch
        {
            ValidationException validationException => (
                StatusCodes.Status400BadRequest,
                new HttpValidationProblemDetails(validationException.Errors.ToDictionary())
                {
                    Title = "Validierungsfehler",
                    Status = StatusCodes.Status400BadRequest
                }),
            NotFoundException notFoundException => (
                StatusCodes.Status404NotFound,
                new ProblemDetails
                {
                    Title = "Nicht gefunden",
                    Detail = notFoundException.Message,
                    Status = StatusCodes.Status404NotFound
                }),
            ConflictException conflictException => (
                StatusCodes.Status409Conflict,
                new ProblemDetails
                {
                    Title = "Konflikt",
                    Detail = conflictException.Message,
                    Status = StatusCodes.Status409Conflict
                }),
            _ => (
                StatusCodes.Status500InternalServerError,
                new ProblemDetails
                {
                    Title = "Serverfehler",
                    Status = StatusCodes.Status500InternalServerError
                })
        };
    }
}
