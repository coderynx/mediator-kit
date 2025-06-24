using Coderynx.MediatorKit.Abstractions;

namespace Coderynx.CqrsKit.Tests;

public class TrackingBehavior : IPipelineBehavior<IRequest<int>, int>
{
    public static bool WasCalled { get; private set; }

    public Task<int> HandleAsync(
        IRequest<int> request,
        RequestHandlerDelegate<int> next,
        CancellationToken cancellationToken = default)
    {
        WasCalled = true;
        return next();
    }
}