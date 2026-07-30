namespace MyMusic.Application.Tests.Common.Exceptions;

public class ExceptionManagerTests
{
    private readonly ExceptionManager _exceptionManager = new();

    [Fact]
    public void ValidationFailed_GruppiertFehlermeldungenNachPropertyName()
    {
        var failures = new[]
        {
            new ValidationFailure("Name", "Der Name darf nicht leer sein."),
            new ValidationFailure("Name", "Der Name ist zu lang."),
            new ValidationFailure("Year", "Das Jahr ist ungültig.")
        };

        var exception = _exceptionManager.ValidationFailed(failures);

        Assert.Equal(2, exception.Errors.Count);
        Assert.Equal(2, exception.Errors["Name"].Length);
        Assert.Single(exception.Errors["Year"]);
    }

    [Fact]
    public void NotFound_ErzeugtDeutscheFehlermeldungMitEntitaetUndId()
    {
        var id = Guid.NewGuid();

        var exception = _exceptionManager.NotFound("Genre", id);

        Assert.Equal($"Genre mit der Id '{id}' wurde nicht gefunden.", exception.Message);
    }

    [Fact]
    public void NotFound_UnterstuetztAuchNichtGuidSchluessel()
    {
        var exception = _exceptionManager.NotFound("Genre", 42);

        Assert.Equal("Genre mit der Id '42' wurde nicht gefunden.", exception.Message);
    }

    [Fact]
    public void Conflict_UebernimmtDieUebergebeneNachricht()
    {
        var exception = _exceptionManager.Conflict("Ein Genre mit diesem Namen existiert bereits.");

        Assert.Equal("Ein Genre mit diesem Namen existiert bereits.", exception.Message);
    }
}
