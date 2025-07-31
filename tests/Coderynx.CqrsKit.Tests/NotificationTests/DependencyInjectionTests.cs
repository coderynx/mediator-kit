using Coderynx.CqrsKit.Tests.RequestTests;
using Coderynx.MediatorKit;
using Coderynx.MediatorKit.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSubstitute;

namespace Coderynx.CqrsKit.Tests.NotificationTests;

public sealed class DependencyInjectionTests
{
    [Fact]
    public void AddMediatorKit_ShouldRegisterPublisher_WhenCalled()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = Substitute.For<IHostApplicationBuilder>();
        builder.Services.Returns(services);

        // Act
        builder.Services.AddMediatorKit();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(IPublisher));
        Assert.NotNull(serviceDescriptor);
        Assert.Equal(typeof(Publisher), serviceDescriptor.ImplementationType);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void AddPipelineBehavior_Generic_ShouldRegisterBehavior()
    {
        // Arrange
        var services = new ServiceCollection();
        var cqrsBuilder = new MediatorKitBuilder(services);

        // Act
        cqrsBuilder.AddNotificationPipelineBehavior<
            TestNotificationPipelineBehavior<TestNotification>,
            TestNotification>();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(s =>
            s.ServiceType == typeof(INotificationPipelineBehavior<TestNotification>));

        Assert.NotNull(serviceDescriptor);
        Assert.Equal(typeof(TestNotificationPipelineBehavior<TestNotification>), serviceDescriptor.ImplementationType);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void AddPipelineBehavior_Type_ShouldRegisterBehavior_WhenValidType()
    {
        // Arrange
        var services = new ServiceCollection();
        var cqrsBuilder = new MediatorKitBuilder(services);
        var behaviorType = typeof(TestRequestPipelineBehavior<,>);

        // Act
        cqrsBuilder.AddRequestPipelineBehavior(behaviorType);

        // Assert
        var serviceDescriptor = services.FirstOrDefault(s =>
            s.ServiceType == typeof(IRequestPipelineBehavior<,>));
        Assert.NotNull(serviceDescriptor);
        Assert.Equal(behaviorType, serviceDescriptor.ImplementationType);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void AddPipelineBehavior_Type_ShouldThrowException_WhenTypeNotGeneric()
    {
        // Arrange
        var services = new ServiceCollection();
        var cqrsBuilder = new MediatorKitBuilder(services);
        var nonGenericType = typeof(string);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            cqrsBuilder.AddNotificationPipelineBehavior(nonGenericType));
        Assert.Equal("Behavior must be an open generic type, e.g. typeof(LoggingBehavior<>)", exception.Message);
    }

    [Fact]
    public void AddPipelineBehavior_Type_ShouldThrowException_WhenTypeDoesNotImplementInterface()
    {
        // Arrange
        var services = new ServiceCollection();
        var cqrsBuilder = new MediatorKitBuilder(services);
        var invalidType = typeof(List<>);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            cqrsBuilder.AddNotificationPipelineBehavior(invalidType));

        Assert.Equal(
            "Behavior type must implement INotificationPipelineBehavior<> (Parameter 'behaviorType')",
            exception.Message);
    }

    [Fact]
    public void AddHandlers_ShouldRegisterNotificationHandlers()
    {
        // Arrange
        var services = new ServiceCollection();
        var cqrsBuilder = new MediatorKitBuilder(services);

        // Act
        cqrsBuilder.AddNotificationHandlers<TestNotificationHandler>();

        // Assert
        var notificationHandler = services
            .FirstOrDefault(s => s.ServiceType == typeof(INotificationHandler<TestNotification>));

        Assert.NotNull(notificationHandler);
        Assert.Equal(typeof(TestNotificationHandler), notificationHandler.ImplementationType);
        Assert.Equal(ServiceLifetime.Scoped, notificationHandler.Lifetime);
    }

    [Fact]
    public async Task SendAsync_ShouldReturnResult()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddMediatorKit(mediator => { mediator.AddNotificationHandlers<TestNotificationHandler>(); });

        var provider = services.BuildServiceProvider();
        var publisher = provider.GetRequiredService<IPublisher>();

        // Act
        await publisher.PublishAsync(new TestNotification());
    }
}