namespace MyMusic.Application.Tests.Common.CQRS.TestDoubles;

public sealed class SampleCommandHandler : ICommandHandler<SampleCommand, string>
{
    public Task<string> HandleAsync(SampleCommand command, CancellationToken cancellationToken)
    {
        return Task.FromResult("Command-Ergebnis");
    }
}
