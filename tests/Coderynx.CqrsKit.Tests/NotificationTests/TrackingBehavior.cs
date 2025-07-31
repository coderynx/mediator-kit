using Coderynx.MediatorKit.Abstractions;

namespace Coderynx.CqrsKit.Tests.NotificationTests;

public class NotificationTrackingBehavior<TNotification> : INotificationPipelineBehavior<TNotification>
    where TNotification : INotification
{
    // ReSharper disable once StaticMemberInGenericType
    public static bool WasCalled { get; private set; }

    public Task HandleAsync(
        TNotification notification,
        NotificationHandlerDelegate next,
        CancellationToken cancellationToken = default)
    {
        WasCalled = true;
        return next();
    }
}