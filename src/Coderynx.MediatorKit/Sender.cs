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
        var type = request switch
        {
            ICommand<TResponse> => typeof(ICommandHandler<,>),
            IQuery<TResponse> => typeof(IQueryHandler<,>),
            _ => throw new ArgumentOutOfRangeException(nameof(request), request, null)
        };

        var handlerType = type.MakeGenericType(request.GetType(), typeof(TResponse));

        dynamic? handler = serviceProvider.GetService(handlerType);
        if (handler is null)
        {
            throw new InvalidOperationException($"Handler for '{request.GetType().Name}' not found.");
        }

        var requestType = request.GetType();
        var behaviorInterfaces = new[]
        {
            typeof(IPipelineBehavior<,>).MakeGenericType(requestType, typeof(TResponse)),
            typeof(IPipelineBehavior<IRequest<TResponse>, TResponse>),
            request is ICommand<TResponse>
                ? typeof(IPipelineBehavior<ICommand<TResponse>, TResponse>)
                : null,
            request is IQuery<TResponse>
                ? typeof(IPipelineBehavior<IQuery<TResponse>, TResponse>)
                : null
        }.Where(t => t is not null).Cast<Type>();

        var behaviors = behaviorInterfaces
            .SelectMany(serviceProvider.GetServices)
            .Cast<object>()
            .ToList();

        var initialDelegate = (RequestHandlerDelegate<TResponse>)(()
            => handler.HandleAsync((dynamic)request, cancellationToken));

        var pipelineDelegate = behaviors
            .Reverse<object>()
            .Aggregate(
                initialDelegate,
                (next, behavior) => () =>
                {
                    dynamic dynBehavior = behavior;
                    return dynBehavior.HandleAsync((dynamic)request, next, cancellationToken);
                });

        return await pipelineDelegate();
    }
}