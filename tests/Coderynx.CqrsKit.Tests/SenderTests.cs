using Coderynx.Functional.Results;
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
        services.AddTransient<ICommandHandler<TestCommand, Result<int>>, TestCommandHandler>();
        services.AddTransient<IQueryHandler<TestQuery, Result<int>>, TestQueryHandler>();
        services.AddTransient<IPipelineBehavior<IRequest<Result>, Result>, LoggingBehavior>();

        _provider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task SendAsync_Command_ReturnsSuccessResult()
    {
        // Arrange
        var sender = new Sender(_provider);

        // Act
        var result = await sender.SendAsync(new TestCommand());

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Success.Value);
    }

    [Fact]
    public async Task SendAsync_Query_ReturnsSuccessResult()
    {
        // Arrange
        var sender = new Sender(_provider);

        // Act
        var result = await sender.SendAsync(new TestQuery());

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Success.Value);
    }

    [Fact]
    public async Task SendAsync_HandlerNotRegistered_Throws()
    {
        // Arrange
        var emptyProvider = new ServiceCollection().BuildServiceProvider();
        var sender = new Sender(emptyProvider);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sender.SendAsync(new TestCommand()));

        Assert.Contains("Handler for 'TestCommand' not found", ex.Message);
    }

    [Fact]
    public async Task SendAsync_PipelineBehaviorIsCalled()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<ICommandHandler<TestCommand, Result<int>>, TestCommandHandler>();
        services.AddSingleton<IPipelineBehavior<IRequest<Result<int>>, Result<int>>, TrackingBehavior>();

        var provider = services.BuildServiceProvider();
        var sender = new Sender(provider);

        // Act
        var result = await sender.SendAsync(new TestCommand());

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Success.Value);
        Assert.True(TrackingBehavior.WasCalled);
    }

    [Fact]
    public async Task SendAsync_GenericTrackingBehavior_IsCalled()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<ICommandHandler<TestCommand, Result<int>>, TestCommandHandler>();
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(GenericTrackingBehavior<,>));

        var provider = services.BuildServiceProvider();
        var sender = new Sender(provider);

        // Act
        var result = await sender.SendAsync(new TestCommand());

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Success.Value);
        Assert.True(GenericTrackingBehavior<TestCommand, Result<int>>.WasCalled);
    }
}