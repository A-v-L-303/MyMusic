namespace MyMusic.Application.Tests.Common.Exceptions;

public class ExceptionManagerTests
{
    private readonly ExceptionManager _exceptionManager = new();

    [Fact]
    public void ValidationFailed_GruppiertFehlermeldungenNachPropertyName()
    {
        // arrange
        var failures = new[]
        {
            new ValidationFailure("Name", "Der Name darf nicht leer sein."),
            new ValidationFailure("Name", "Der Name ist zu lang."),
            new ValidationFailure("Year", "Das Jahr ist ungültig.")
        };

        // act
        var exception = _exceptionManager.ValidationFailed(failures);

        // assert
        Assert.Equal(2, exception.Errors.Count);
        Assert.Equal(2, exception.Errors["Name"].Length);
        Assert.Single(exception.Errors["Year"]);
    }

    [Fact]
    public void NotFound_ErzeugtDeutscheFehlermeldungMitEntitaetUndId()
    {
        // arrange
        var id = Guid.NewGuid();

        // act
        var exception = _exceptionManager.NotFound("Genre", id);

        // assert
        Assert.Equal($"Genre mit der Id '{id}' wurde nicht gefunden.", exception.Message);
    }

    [Fact]
    public void Conflict_UebernimmtDieUebergebeneNachricht()
    {
        // arrange

        // act
        var exception = _exceptionManager.Conflict("Ein Genre mit diesem Namen existiert bereits.");

        // assert
        Assert.Equal("Ein Genre mit diesem Namen existiert bereits.", exception.Message);
    }
}
