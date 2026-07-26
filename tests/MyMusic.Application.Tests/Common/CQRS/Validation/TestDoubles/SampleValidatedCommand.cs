namespace MyMusic.Application.Tests.Common.CQRS.Validation.TestDoubles;

public sealed class SampleValidatedCommand : ICommand<string>
{
    public string? Name { get; init; }
}
