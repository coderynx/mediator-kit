using Coderynx.MediatorKit;
using Coderynx.MediatorKit.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Coderynx.CqrsKit.Tests;

public class SenderTests
{
    private readonly IServiceProvider _provider;

    public SenderTests()
    {
        var services = new ServiceCollection();
        services.AddTransient<IRequestHandler<TestRequest, int>, TestRequestHandler>();
        services.AddTransient<IPipelineBehavior<IRequest<int>, int>, LoggingBehavior>();

        _provider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task SendAsync_ReturnsSuccessResult()
    {
        // Arrange
        var sender = new Sender(_provider);

        // Act
        var result = await sender.SendAsync(new TestRequest());

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public async Task SendAsync_HandlerNotRegistered_Throws()
    {
        // Arrange
        var emptyProvider = new ServiceCollection().BuildServiceProvider();
        var sender = new Sender(emptyProvider);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => sender.SendAsync(new TestRequest()));
        Assert.Contains("No handler found for request of type 'Coderynx.CqrsKit.Tests.TestRequest'", ex.Message);
    }

    [Fact]
    public async Task SendAsync_PipelineBehaviorIsCalled()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IRequestHandler<TestRequest, int>, TestRequestHandler>();
        services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(TrackingBehavior<,>));

        var provider = services.BuildServiceProvider();
        var sender = new Sender(provider);

        // Act
        var result = await sender.SendAsync(new TestRequest());

        // Assert
        Assert.Equal(1, result);
        Assert.True(TrackingBehavior<TestRequest, int>.WasCalled);
    }

    [Fact]
    public async Task SendAsync_GenericTrackingBehavior_IsCalled()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IRequestHandler<TestRequest, int>, TestRequestHandler>();
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(GenericTrackingBehavior<,>));

        var provider = services.BuildServiceProvider();
        var sender = new Sender(provider);

        // Act
        var result = await sender.SendAsync(new TestRequest());

        // Assert
        Assert.Equal(1, result);
        Assert.True(GenericTrackingBehavior<TestRequest, int>.WasCalled);
    }
}