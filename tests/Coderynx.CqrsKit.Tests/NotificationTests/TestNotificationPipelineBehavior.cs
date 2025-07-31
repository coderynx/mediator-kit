using Coderynx.MediatorKit.Abstractions;

namespace Coderynx.CqrsKit.Tests.NotificationTests;

public class TestNotificationPipelineBehavior<TNotification> : INotificationPipelineBehavior<TNotification>
    where TNotification : INotification
{
    public Task HandleAsync(
        TNotification notification,
        NotificationHandlerDelegate next,
        CancellationToken cancellationToken = default)
    {
        return next();
    }
}