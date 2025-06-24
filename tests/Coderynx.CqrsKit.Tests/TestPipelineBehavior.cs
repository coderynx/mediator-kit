using Coderynx.Functional.Results;
using Coderynx.MediatorKit.Abstractions;

namespace Coderynx.CqrsKit.Tests;

public class TestPipelineBehavior<TCommand, TResult> : IPipelineBehavior<TCommand, TResult>
    where TCommand : IRequest<TResult>
    where TResult : Result
{
    public async Task<TResult> HandleAsync(
        TCommand request,
        RequestHandlerDelegate<TResult> next,
        CancellationToken cancellationToken = default)
    {
        return await next();
    }
}