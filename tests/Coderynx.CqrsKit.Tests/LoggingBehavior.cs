using Coderynx.Functional.Results;
using Coderynx.MediatorKit.Abstractions;

namespace Coderynx.CqrsKit.Tests;

public sealed class LoggingBehavior : IPipelineBehavior<IRequest<Result>, Result>
{
    public async Task<Result> HandleAsync(IRequest<Result> request, RequestHandlerDelegate<Result> next,
        CancellationToken cancellationToken)
    {
        return await next();
    }
}