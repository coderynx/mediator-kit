using Coderynx.MediatorKit.Abstractions;

namespace Coderynx.CqrsKit.Tests.NotificationTests;

public sealed record TestNotification : INotification;

public class TestNotificationHandler : INotificationHandler<TestNotification>
{
    public Task HandleAsync(TestNotification request, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}