using Coderynx.Functional.Results;
using Coderynx.Functional.Results.Successes;
using Coderynx.MediatorKit.Abstractions;

namespace Coderynx.CqrsKit.Tests;

public sealed class TestQuery : IQuery<Result<int>>;

public class TestQueryHandler : IQueryHandler<TestQuery, Result<int>>
{
    public Task<Result<int>> HandleAsync(TestQuery query, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<Result<int>>(Success.Created(1));
    }
}