using Coderynx.Functional.Results;
using Coderynx.MediatorKit.Abstractions;

namespace Coderynx.CqrsKit.Tests;

public class GenericTrackingBehavior<TCommand, TResult>
    : IPipelineBehavior<TCommand, TResult>
    where TCommand : IRequest<TResult>
    where TResult : Result
{
    // ReSharper disable once StaticMemberInGenericType
    public static bool WasCalled { get; private set; }

    public Task<TResult> HandleAsync(
        TCommand request,
        RequestHandlerDelegate<TResult> next,
        CancellationToken cancellationToken = default)
    {
        WasCalled = true;
        return next();
    }
}