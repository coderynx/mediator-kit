using Coderynx.MediatorKit.Abstractions;

namespace Coderynx.MediatorKit.Tests.RequestTests;

public sealed class RequestLoggingBehavior : IRequestPipelineBehavior<IRequest<int>, int>
{
    public async Task<int> HandleAsync(IRequest<int> request, RequestHandlerDelegate<int> next,
        CancellationToken cancellationToken)
    {
        return await next();
    }
}