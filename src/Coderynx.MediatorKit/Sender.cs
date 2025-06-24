using Coderynx.MediatorKit.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Coderynx.MediatorKit;

internal sealed class Sender(IServiceProvider serviceProvider) : ISender
{
    public async Task<TResponse> SendAsync<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var handlerInterface = typeof(IRequestHandler<,>);

        var requestType = request.GetType();
        var handlerType = handlerInterface.MakeGenericType(requestType, typeof(TResponse));
        var handler = serviceProvider.GetService(handlerType) ??
                      throw new InvalidOperationException($"Handler for request type '{requestType.Name}' not found.");

        var handleMethod = handlerType.GetMethod("HandleAsync") ??
                           throw new InvalidOperationException($"'{handlerType.Name}' does not implement HandleAsync.");

        var pipelineBehaviorTypes = new List<Type>
        {
            typeof(IPipelineBehavior<,>).MakeGenericType(requestType, typeof(TResponse)),
            typeof(IPipelineBehavior<IRequest<TResponse>, TResponse>)
        };

        var behaviors = pipelineBehaviorTypes
            .SelectMany(type => (IEnumerable<object>)serviceProvider.GetServices(type))
            .ToList();

        foreach (var behavior in from behavior in behaviors
                 let method = behavior.GetType().GetMethod("HandleAsync")
                 where method is null
                 select behavior)
        {
            throw new InvalidOperationException(
                $"Pipeline behavior {behavior.GetType().Name} does not implement HandleAsync.");
        }

        var pipeline = behaviors
            .Reverse<object>()
            .Aggregate((RequestHandlerDelegate<TResponse>)HandlerDelegate, (next, behavior) => () =>
            {
                dynamic dynBehavior = behavior;
                return dynBehavior.HandleAsync((dynamic)request, next, cancellationToken);
            });

        return await pipeline();

        Task<TResponse> HandlerDelegate()
        {
            return (Task<TResponse>)handleMethod.Invoke(handler, [request, cancellationToken])!;
        }
    }
}