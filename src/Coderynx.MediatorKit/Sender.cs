using Coderynx.Functional.Results;
using Coderynx.MediatorKit.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Coderynx.MediatorKit;

internal sealed class Sender(IServiceProvider serviceProvider) : ISender
{
    public async Task<TResponse> SendAsync<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellationToken = default) where TResponse : Result
    {
        ArgumentNullException.ThrowIfNull(request);

        var handlerInterface = request switch
        {
            ICommand<TResponse> => typeof(ICommandHandler<,>),
            IQuery<TResponse> => typeof(IQueryHandler<,>),
            _ => throw new ArgumentOutOfRangeException(nameof(request),
                $"Unknown request type: {request.GetType().Name}")
        };

        var requestType = request.GetType();
        var handlerType = handlerInterface.MakeGenericType(requestType, typeof(TResponse));
        var handler = serviceProvider.GetService(handlerType) ??
                      throw new InvalidOperationException($"Handler for '{requestType.Name}' not found.");

        var handleMethod = handlerType.GetMethod("HandleAsync") ??
                           throw new InvalidOperationException($"'{handlerType.Name}' does not implement HandleAsync.");

        var pipelineBehaviorTypes = new List<Type>
        {
            typeof(IPipelineBehavior<,>).MakeGenericType(requestType, typeof(TResponse)),
            typeof(IPipelineBehavior<IRequest<TResponse>, TResponse>)
        };

        switch (request)
        {
            case ICommand<TResponse>:
                pipelineBehaviorTypes.Add(typeof(IPipelineBehavior<ICommand<TResponse>, TResponse>));
                break;
            case IQuery<TResponse>:
                pipelineBehaviorTypes.Add(typeof(IPipelineBehavior<IQuery<TResponse>, TResponse>));
                break;
        }

        var behaviors = pipelineBehaviorTypes
            .SelectMany(serviceProvider.GetServices)
            .Cast<object>()
            .ToList();

        var pipeline = behaviors
            .Reverse<object>()
            .Aggregate((RequestHandlerDelegate<TResponse>)HandlerDelegate, (next, behavior) => () =>
            {
                dynamic dynBehavior = behavior;
                return dynBehavior.HandleAsync((dynamic)request, next, cancellationToken);
            });

        return await pipeline();

        Task<TResponse> HandlerDelegate() =>
            (Task<TResponse>)handleMethod.Invoke(handler, [request, cancellationToken])!;
    }
}