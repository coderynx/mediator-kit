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
        services.AddTransient<ICommandHandler<FakeCommand, Result<int>>, FakeCommandHandler>();
        services.AddTransient<IQueryHandler<FakeQuery, Result<int>>, FakeQueryHandler>();
        services.AddTransient<IPipelineBehavior<IRequest<Result>, Result>, LoggingBehavior>();

        _provider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task SendAsync_Command_ReturnsSuccessResult()
    {
        // Arrange
        var sender = new Sender(_provider);

        // Act
        var result = await sender.SendAsync<Result<int>>(new FakeCommand());

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
        var result = await sender.SendAsync<Result<int>>(new FakeQuery());

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
            sender.SendAsync<Result<int>>(new FakeCommand()));

        Assert.Contains("Handler for 'FakeCommand' not found", ex.Message);
    }

    [Fact]
    public async Task SendAsync_PipelineBehaviorIsCalled()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<ICommandHandler<FakeCommand, Result<int>>, FakeCommandHandler>();
        services.AddSingleton<IPipelineBehavior<IRequest<Result<int>>, Result<int>>, TrackingBehavior>();

        var provider = services.BuildServiceProvider();
        var sender = new Sender(provider);

        // Act
        var result = await sender.SendAsync<Result<int>>(new FakeCommand());

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
        services.AddTransient<ICommandHandler<FakeCommand, Result<int>>, FakeCommandHandler>();
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(GenericTrackingBehavior<,>));

        var provider = services.BuildServiceProvider();
        var sender = new Sender(provider);

        // Act
        var result = await sender.SendAsync<Result<int>>(new FakeCommand());

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.Success.Value);
        Assert.True(GenericTrackingBehavior<FakeCommand, Result<int>>.WasCalled);
    }

    public sealed class FakeCommand : IRequest<Result>, ICommand<Result<int>>;

    public sealed class FakeQuery : IRequest<Result>, IQuery<Result<int>>;

    public sealed class FakeCommandHandler : ICommandHandler<FakeCommand, Result<int>>
    {
        public Task<Result<int>> HandleAsync(FakeCommand request, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result.Created(1));
        }
    }

    public sealed class FakeQueryHandler : IQueryHandler<FakeQuery, Result<int>>
    {
        public Task<Result<int>> HandleAsync(FakeQuery request, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result.Created(1));
        }
    }

    public sealed class LoggingBehavior : IPipelineBehavior<IRequest<Result>, Result>
    {
        public async Task<Result> HandleAsync(IRequest<Result> request, RequestHandlerDelegate<Result> next,
            CancellationToken cancellationToken)
        {
            return await next();
        }
    }

    public class TrackingBehavior : IPipelineBehavior<IRequest<Result<int>>, Result<int>>
    {
        public static bool WasCalled { get; private set; }

        public Task<Result<int>> HandleAsync(
            IRequest<Result<int>> request,
            RequestHandlerDelegate<Result<int>> next,
            CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            return next();
        }
    }

    public class GenericTrackingBehavior<TCommand, TResult>
        : IPipelineBehavior<TCommand, TResult>
        where TCommand : IRequest<TResult>
        where TResult : Result
    {
        // ReSharper disable once StaticMemberInGenericType
        public static bool WasCalled { get; private set; }

        public Task<TResult> HandleAsync(
            TCommand request,
            RequestHandlerDelegate<TResult> next,
            CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            return next();
        }
    }
}