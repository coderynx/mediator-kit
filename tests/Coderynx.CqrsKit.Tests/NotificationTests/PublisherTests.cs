using Coderynx.MediatorKit;
using Coderynx.MediatorKit.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Coderynx.CqrsKit.Tests.NotificationTests;

public class PublisherTests
{
    private readonly IServiceProvider _provider;

    public PublisherTests()
    {
        var services = new ServiceCollection();
        services.AddTransient<INotificationHandler<TestNotification>, TestNotificationHandler>();
        services.AddTransient<INotificationPipelineBehavior<INotification>, NotificationLoggingBehavior>();

        _provider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task PublishAsync_Succeed()
    {
        // Arrange
        var publisher = new Publisher(_provider);

        // Act
        await publisher.PublishAsync(new TestNotification());
    }

    [Fact]
    public async Task PublishAsync_HandlerNotRegistered_Throws()
    {
        // Arrange
        var emptyProvider = new ServiceCollection().BuildServiceProvider();
        var publisher = new Publisher(emptyProvider);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            publisher.PublishAsync(new TestNotification()));

        Assert.Contains(
            "No handler found for notification of type 'Coderynx.CqrsKit.Tests.NotificationTests.TestNotification'",
            exception.Message);
    }

    [Fact]
    public async Task PublishAsync_PipelineBehaviorIsCalled()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<INotificationHandler<TestNotification>, TestNotificationHandler>();
        services.AddSingleton(typeof(INotificationPipelineBehavior<>), typeof(NotificationTrackingBehavior<>));

        var provider = services.BuildServiceProvider();
        var publisher = new Publisher(provider);

        // Act
        await publisher.PublishAsync(new TestNotification());

        // Assert
        Assert.True(NotificationTrackingBehavior<TestNotification>.WasCalled);
    }

    [Fact]
    public async Task PublishAsync_GenericTrackingBehavior_IsCalled()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<INotificationHandler<TestNotification>, TestNotificationHandler>();
        services.AddTransient(typeof(INotificationPipelineBehavior<>), typeof(GenericTrackingNotificationBehavior<>));

        var provider = services.BuildServiceProvider();
        var publisher = new Publisher(provider);

        // Act
        await publisher.PublishAsync(new TestNotification());

        // Assert
        Assert.True(GenericTrackingNotificationBehavior<TestNotification>.WasCalled);
    }
}