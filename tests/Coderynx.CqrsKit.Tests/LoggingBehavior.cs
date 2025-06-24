using Coderynx.MediatorKit.Abstractions;

namespace Coderynx.CqrsKit.Tests;

public sealed class LoggingBehavior : IPipelineBehavior<IRequest<int>, int>
{
    public async Task<int> HandleAsync(IRequest<int> request, RequestHandlerDelegate<int> next,
        CancellationToken cancellationToken)
    {
        return await next();
    }
}