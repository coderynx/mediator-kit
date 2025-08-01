using Coderynx.MediatorKit.Abstractions;

namespace Coderynx.MediatorKit.Tests.NotificationTests;

public sealed class NotificationLoggingBehavior : INotificationPipelineBehavior<INotification>
{
    public Task HandleAsync(
        INotification notification,
        NotificationHandlerDelegate next,
        CancellationToken cancellationToken = default)
    {
        return next();
    }
}