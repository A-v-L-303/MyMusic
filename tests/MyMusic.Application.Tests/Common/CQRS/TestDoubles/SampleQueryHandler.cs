namespace MyMusic.Application.Tests.Common.CQRS.TestDoubles;

public sealed class SampleQueryHandler : IQueryHandler<SampleQuery, int>
{
    public Task<int> HandleAsync(SampleQuery query, CancellationToken cancellationToken)
    {
        return Task.FromResult(42);
    }
}
