using Coderynx.MediatorKit;
using Coderynx.MediatorKit.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NSubstitute;

namespace Coderynx.CqrsKit.Tests.RequestTests;

public sealed class DependencyInjectionTests
{
    [Fact]
    public void AddMediatorKit_ShouldRegisterSender_WhenCalled()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = Substitute.For<IHostApplicationBuilder>();
        builder.Services.Returns(services);

        // Act
        builder.Services.AddMediatorKit();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(ISender));
        Assert.NotNull(serviceDescriptor);
        Assert.Equal(typeof(Sender), serviceDescriptor.ImplementationType);
        Assert.Equal(ServiceLifetime.Scoped, serviceDescriptor.Lifetime);
    }

    [Fact]
    public void AddMediatorKit_ShouldExecuteConfigureAction_WhenProvided()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = Substitute.For<IHostApplicationBuilder>();
        builder.Services.Returns(services);
        var configureWasCalled = false;

        // Act
        builder.Services.AddMediatorKit(cqrsBuilder =>
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
        var cqrsBuilder = new MediatorKitBuilder(services);

        // Act
        cqrsBuilder.AddRequestPipelineBehavior<TestRequestPipelineBehavior<TestRequest, int>, TestRequest, int>();

        // Assert
        var serviceDescriptor = services.FirstOrDefault(s =>
            s.ServiceType == typeof(IRequestPipelineBehavior<TestRequest, int>));
        Assert.NotNull(serviceDescriptor);
        Assert.Equal(typeof(TestRequestPipelineBehavior<TestRequest, int>), serviceDescriptor.ImplementationType);
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
            cqrsBuilder.AddRequestPipelineBehavior(nonGenericType));
        Assert.Equal("Behavior must be an open generic type, e.g. typeof(LoggingBehavior<,>)", exception.Message);
    }

    [Fact]
    public void AddPipelineBehavior_Type_ShouldThrowException_WhenTypeDoesNotImplementInterface()
    {
        // Arrange
        var services = new ServiceCollection();
        var cqrsBuilder = new MediatorKitBuilder(services);
        var invalidType = typeof(List<>);

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => cqrsBuilder.AddRequestPipelineBehavior(invalidType));
        Assert.Equal("Behavior type must implement IPipelineBehavior<,> (Parameter 'behaviorType')", exception.Message);
    }

    [Fact]
    public void AddHandlers_ShouldRegisterRequestHandlers()
    {
        // Arrange
        var services = new ServiceCollection();
        var cqrsBuilder = new MediatorKitBuilder(services);

        // Act
        cqrsBuilder.AddRequestHandlers<TestRequestHandler>();

        // Assert
        var requestHandler = services
            .FirstOrDefault(s => s.ServiceType == typeof(IRequestHandler<TestRequest, int>));
        Assert.NotNull(requestHandler);
        Assert.Equal(typeof(TestRequestHandler), requestHandler.ImplementationType);
        Assert.Equal(ServiceLifetime.Scoped, requestHandler.Lifetime);
    }

    [Fact]
    public async Task SendAsync_ShouldReturnResult()
    {
        // Arrange
        var services = new ServiceCollection();

        services.AddMediatorKit(mediator => { mediator.AddRequestHandlers<TestRequestHandler>(); });

        var provider = services.BuildServiceProvider();
        var sender = provider.GetRequiredService<ISender>();

        // Act
        var result = await sender.SendAsync(new TestRequest());

        // Assert
        Assert.Equal(1, result);
    }
}