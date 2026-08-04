namespace MyMusic.Api.Tests;

public class GlobalExceptionHandlerTests
{
    private readonly GlobalExceptionHandler _handler = new();

    [Fact]
    public async Task TryHandleAsync_MitValidationException_Antwortet400MitFeldfehlern()
    {
        // arrange
        var errors = new Dictionary<string, string[]> { ["Name"] = ["Der Name darf nicht leer sein."] };

        var exception = new ValidationException(errors);

        var httpContext = CreateHttpContext();

        // act
        var handled = await _handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        // assert
        Assert.True(handled);

        Assert.Equal(StatusCodes.Status400BadRequest, httpContext.Response.StatusCode);

        var problemDetails = await ReadProblemDetailsAsync<HttpValidationProblemDetails>(httpContext);

        Assert.Contains("Name", problemDetails.Errors.Keys);
    }

    [Fact]
    public async Task TryHandleAsync_MitNotFoundException_Antwortet404()
    {
        // arrange
        var exception = new NotFoundException("Genre mit der Id 'x' wurde nicht gefunden.");

        var httpContext = CreateHttpContext();

        // act
        await _handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        // assert
        Assert.Equal(StatusCodes.Status404NotFound, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task TryHandleAsync_MitConflictException_Antwortet409()
    {
        // arrange
        var exception = new ConflictException("Konflikt.");

        var httpContext = CreateHttpContext();

        // act
        await _handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        // assert
        Assert.Equal(StatusCodes.Status409Conflict, httpContext.Response.StatusCode);
    }

    [Fact]
    public async Task TryHandleAsync_MitUnbekannterException_Antwortet500()
    {
        // arrange
        var exception = new InvalidOperationException("Unerwarteter Fehler.");

        var httpContext = CreateHttpContext();

        // act
        await _handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        // assert
        Assert.Equal(StatusCodes.Status500InternalServerError, httpContext.Response.StatusCode);
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        return new DefaultHttpContext { Response = { Body = new MemoryStream() } };
    }

    private static async Task<TProblemDetails> ReadProblemDetailsAsync<TProblemDetails>(HttpContext httpContext)
    {
        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        var problemDetails = await JsonSerializer.DeserializeAsync<TProblemDetails>(httpContext.Response.Body, options);

        Assert.NotNull(problemDetails);

        return problemDetails;
    }
}
