using Coderynx.Functional.Results;
using Coderynx.Functional.Results.Successes;
using Coderynx.MediatorKit;
using Coderynx.MediatorKit.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSubstitute;

namespace Coderynx.CqrsKit.Tests;

public sealed class DependencyInjectionTests
{
    [Fact]
    public void AddCqrs_ShouldRegisterSender_WhenCalled()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = Substitute.For<IHostApplicationBuilder>();
        builder.Services.Returns(services);

        // Act
        builder.AddMediatorKit();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(ISender));
        Assert.NotNull(serviceDescriptor);
        Assert.Equal(typeof(Sender), serviceDescriptor.ImplementationType);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void AddCqrs_ShouldExecuteConfigureAction_WhenProvided()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = Substitute.For<IHostApplicationBuilder>();
        builder.Services.Returns(services);
        var configureWasCalled = false;

        // Act
        builder.AddMediatorKit(cqrsBuilder =>
        {
            configureWasCalled = true;
            Assert.NotNull(cqrsBuilder);
        });

        // Assert
        Assert.True(configureWasCalled);
    }

    [Fact]
    public void AddPipelineBehavior_Generic_ShouldRegisterBehavior()
    {
        // Arrange
        var services = new ServiceCollection();
        var cqrsBuilder = new CqrsBuilder(services);

        // Act
        cqrsBuilder.AddPipelineBehavior<TestPipelineBehavior<TestCommand, Result<int>>, TestCommand, Result<int>>();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(s =>
            s.ServiceType == typeof(IPipelineBehavior<TestCommand, Result<int>>));
        Assert.NotNull(serviceDescriptor);
        Assert.Equal(typeof(TestPipelineBehavior<TestCommand, Result<int>>), serviceDescriptor.ImplementationType);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void AddPipelineBehavior_Type_ShouldRegisterBehavior_WhenValidType()
    {
        // Arrange
        var services = new ServiceCollection();
        var cqrsBuilder = new CqrsBuilder(services);
        var behaviorType = typeof(TestPipelineBehavior<,>);

        // Act
        cqrsBuilder.AddPipelineBehavior(behaviorType);

        // Assert
        var serviceDescriptor = services.FirstOrDefault(s =>
            s.ServiceType == typeof(IPipelineBehavior<,>));
        Assert.NotNull(serviceDescriptor);
        Assert.Equal(behaviorType, serviceDescriptor.ImplementationType);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void AddPipelineBehavior_Type_ShouldThrowException_WhenTypeNotGeneric()
    {
        // Arrange
        var services = new ServiceCollection();
        var cqrsBuilder = new CqrsBuilder(services);
        var nonGenericType = typeof(string);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            cqrsBuilder.AddPipelineBehavior(nonGenericType));
        Assert.Equal("Behavior type must be generic (Parameter 'behaviorType')", exception.Message);
    }

    [Fact]
    public void AddPipelineBehavior_Type_ShouldThrowException_WhenTypeDoesNotImplementInterface()
    {
        // Arrange
        var services = new ServiceCollection();
        var cqrsBuilder = new CqrsBuilder(services);
        var invalidType = typeof(List<>);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            cqrsBuilder.AddPipelineBehavior(invalidType));
        Assert.Equal("Behavior type must implement IPipelineBehavior<,> (Parameter 'behaviorType')", exception.Message);
    }

    [Fact]
    public void AddCommandHandlers_ShouldRegisterAllCommandHandlers()
    {
        // Arrange
        var services = new ServiceCollection();
        var cqrsBuilder = new CqrsBuilder(services);

        // Act
        cqrsBuilder.AddHandlers<TestCommandHandler>();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(s =>
            s.ServiceType == typeof(ICommandHandler<TestCommand, Result<int>>));
        Assert.NotNull(serviceDescriptor);
        Assert.Equal(typeof(TestCommandHandler), serviceDescriptor.ImplementationType);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void AddQueryHandlers_ShouldRegisterAllQueryHandlers()
    {
        // Arrange
        var services = new ServiceCollection();
        var cqrsBuilder = new CqrsBuilder(services);

        // Act
        cqrsBuilder.AddHandlers<TestQueryHandler>();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(s =>
            s.ServiceType == typeof(IQueryHandler<TestQuery, Result<int>>));
        Assert.NotNull(serviceDescriptor);
        Assert.Equal(typeof(TestQueryHandler), serviceDescriptor.ImplementationType);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
    }
}

public sealed class TestCommand : ICommand<Result<int>>;

public sealed class TestQuery : IQuery<Result<int>>;

public class TestPipelineBehavior<TCommand, TResult> : IPipelineBehavior<TCommand, TResult>
    where TCommand : IRequest<TResult>
    where TResult : Result
{
    public async Task<TResult> HandleAsync(
        TCommand request,
        RequestHandlerDelegate<TResult> next,
        CancellationToken cancellationToken = default)
    {
        return await next();
    }
}

public class TestCommandHandler : ICommandHandler<TestCommand, Result<int>>
{
    public Task<Result<int>> HandleAsync(TestCommand request, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<Result<int>>(Success.Created(1));
    }
}

public class TestQueryHandler : IQueryHandler<TestQuery, Result<int>>
{
    public Task<Result<int>> HandleAsync(TestQuery query, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<Result<int>>(Success.Created(1));
    }
}