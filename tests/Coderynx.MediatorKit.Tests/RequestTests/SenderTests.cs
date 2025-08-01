using Coderynx.MediatorKit;
using Coderynx.MediatorKit.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Coderynx.MediatorKit.Tests.RequestTests;

public class SenderTests
{
    private readonly IServiceProvider _provider;

    public SenderTests()
    {
        var services = new ServiceCollection();
        services.AddTransient<IRequestHandler<TestRequest, int>, TestRequestHandler>();
        services.AddTransient<IRequestPipelineBehavior<IRequest<int>, int>, RequestLoggingBehavior>();

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
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => sender.SendAsync(new TestRequest()));

        Assert.Contains(
            "No handler found for request of type 'Coderynx.MediatorKit.Tests.RequestTests.TestRequest'",
            exception.Message);
    }

    [Fact]
    public async Task SendAsync_PipelineBehaviorIsCalled()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IRequestHandler<TestRequest, int>, TestRequestHandler>();
        services.AddSingleton(typeof(IRequestPipelineBehavior<,>), typeof(RequestTrackingBehavior<,>));

        var provider = services.BuildServiceProvider();
        var sender = new Sender(provider);

        // Act
        var result = await sender.SendAsync(new TestRequest());

        // Assert
        Assert.Equal(1, result);
        Assert.True(RequestTrackingBehavior<TestRequest, int>.WasCalled);
    }

    [Fact]
    public async Task SendAsync_GenericTrackingBehavior_IsCalled()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IRequestHandler<TestRequest, int>, TestRequestHandler>();
        services.AddTransient(typeof(IRequestPipelineBehavior<,>), typeof(GenericTrackingRequestBehavior<,>));

        var provider = services.BuildServiceProvider();
        var sender = new Sender(provider);

        // Act
        var result = await sender.SendAsync(new TestRequest());

        // Assert
        Assert.Equal(1, result);
        Assert.True(GenericTrackingRequestBehavior<TestRequest, int>.WasCalled);
    }
}