using Coderynx.Functional.Results;
using Coderynx.MediatorKit.Abstractions;

namespace Coderynx.CqrsKit.Tests;

public class TrackingBehavior : IPipelineBehavior<IRequest<Result<int>>, Result<int>>
{
    public static bool WasCalled { get; private set; }

    public Task<Result<int>> HandleAsync(
        IRequest<Result<int>> request,
        RequestHandlerDelegate<Result<int>> next,
        CancellationToken cancellationToken = default)
    {
        WasCalled = true;
        return next();
    }
}