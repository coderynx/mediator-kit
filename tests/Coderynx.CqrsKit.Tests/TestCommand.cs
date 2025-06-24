using Coderynx.Functional.Results;
using Coderynx.Functional.Results.Successes;
using Coderynx.MediatorKit.Abstractions;

namespace Coderynx.CqrsKit.Tests;

public sealed class TestCommand : ICommand<Result<int>>;

public class TestCommandHandler : ICommandHandler<TestCommand, Result<int>>
{
    public Task<Result<int>> HandleAsync(TestCommand request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<Result<int>>(Success.Created(1));
    }
}