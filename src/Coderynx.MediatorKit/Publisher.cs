using Coderynx.MediatorKit.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Coderynx.MediatorKit;

public sealed class Publisher(IServiceProvider serviceProvider) : IPublisher
{
    public Task PublishAsync(INotification notification, CancellationToken cancellationToken = default)
    {
        var notificationType = notification.GetType();
        var handlerType = typeof(INotificationHandler<>).MakeGenericType(notificationType);

        var handler = serviceProvider.GetService(handlerType);
        if (handler is null)
        {
            throw new InvalidOperationException(
                $"No handler found for notification of type '{notification.GetType()}'");
        }

        NotificationHandlerDelegate handlerDelegate = () =>
            (Task)handlerType
                .GetMethod("HandleAsync")!
                .Invoke(handler, [notification, cancellationToken])!;

        var behaviorType = typeof(INotificationPipelineBehavior<>).MakeGenericType(notificationType);
        var behaviors = serviceProvider
            .GetServices(behaviorType)
            .Reverse()
            .ToList();

        foreach (var behavior in behaviors)
        {
            var next = handlerDelegate;
            handlerDelegate = () => (Task)behaviorType
                .GetMethod("HandleAsync")!
                .Invoke(behavior, [notification, next, cancellationToken])!;
        }

        return handlerDelegate();
    }
}