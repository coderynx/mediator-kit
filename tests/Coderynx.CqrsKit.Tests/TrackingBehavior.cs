using Coderynx.MediatorKit.Abstractions;

namespace Coderynx.CqrsKit.Tests;

public class TrackingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public static bool WasCalled { get; private set; }

    public Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken = new())
    {
        WasCalled = true;
        return next();
    }
}