namespace Coderynx.MediatorKit.Abstractions;

public interface IPublisher
{
    Task PublishAsync(INotification notification, CancellationToken cancellationToken = default);
}