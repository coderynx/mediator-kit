using Coderynx.MediatorKit.Abstractions;

namespace Coderynx.CqrsKit.Tests.RequestTests;

public class TestRequestPipelineBehavior<TRequest, TResponse> : IRequestPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> HandleAsync(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken = default)
    {
        return await next();
    }
}