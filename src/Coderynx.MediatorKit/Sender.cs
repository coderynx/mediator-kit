using Coderynx.MediatorKit.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Coderynx.MediatorKit;

public sealed class Sender(IServiceProvider serviceProvider) : ISender
{
    public Task<TResponse> SendAsync<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = new())
    {
        var requestType = request.GetType();
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));
        var handler = serviceProvider.GetService(handlerType);

        if (handler is null)
        {
            throw new InvalidOperationException($"No handler found for request of type '{requestType}'");
        }

        RequestHandlerDelegate<TResponse> handlerDelegate = () =>
            (Task<TResponse>)handlerType
                .GetMethod("HandleAsync")!
                .Invoke(handler, [request, cancellationToken])!;

        var behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, typeof(TResponse));
        var behaviors = serviceProvider
            .GetServices(behaviorType)
            .Reverse()
            .ToList();

        foreach (var behavior in behaviors)
        {
            var next = handlerDelegate;
            handlerDelegate = () => (Task<TResponse>)behaviorType
                .GetMethod("HandleAsync")!
                .Invoke(behavior, [request, next, cancellationToken])!;
        }

        return handlerDelegate();
    }
}