using Coderynx.MediatorKit.Abstractions;

namespace Coderynx.MediatorKit.Tests.RequestTests;

public class RequestTrackingBehavior<TRequest, TResponse> : IRequestPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    // ReSharper disable once StaticMemberInGenericType
    public static bool WasCalled { get; private set; }

    public Task<TResponse> HandleAsync(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken = new())
    {
        WasCalled = true;
        return next();
    }
}