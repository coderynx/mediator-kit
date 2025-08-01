using Coderynx.MediatorKit.Abstractions;

namespace Coderynx.MediatorKit.Tests.NotificationTests;

public sealed record TestNotification : INotification;

public class TestNotificationHandler : INotificationHandler<TestNotification>
{
    public Task HandleAsync(TestNotification notification, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}