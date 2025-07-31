using Coderynx.MediatorKit.Abstractions;

namespace Coderynx.CqrsKit.Tests.NotificationTests;

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