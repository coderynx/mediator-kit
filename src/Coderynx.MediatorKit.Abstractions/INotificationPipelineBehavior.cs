namespace Coderynx.MediatorKit.Abstractions;

public interface INotificationPipelineBehavior<in TNotification>
    where TNotification : INotification
{
    Task HandleAsync(
        TNotification notification,
        NotificationHandlerDelegate next,
        CancellationToken cancellationToken = default);
}