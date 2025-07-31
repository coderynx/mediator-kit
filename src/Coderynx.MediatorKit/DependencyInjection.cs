using Coderynx.MediatorKit.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Coderynx.MediatorKit;

public class MediatorKitBuilder
{
    internal MediatorKitBuilder(IServiceCollection services)
    {
        Services = services;
    }

    private IServiceCollection Services { get; }

    public MediatorKitBuilder AddRequestPipelineBehavior<TPipelineBehavior, TRequest, TResponse>()
        where TPipelineBehavior : class, IRequestPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        Services.AddScoped<IRequestPipelineBehavior<TRequest, TResponse>, TPipelineBehavior>();
        return this;
    }

    public MediatorKitBuilder AddRequestPipelineBehavior(Type behaviorType)
    {
        if (!behaviorType.IsGenericTypeDefinition)
        {
            throw new ArgumentException("Behavior must be an open generic type, e.g. typeof(LoggingBehavior<,>)");
        }

        if (!behaviorType.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestPipelineBehavior<,>)))
        {
            throw new ArgumentException("Behavior type must implement IPipelineBehavior<,> (Parameter 'behaviorType')");
        }

        Services.AddScoped(typeof(IRequestPipelineBehavior<,>), behaviorType);
        return this;
    }

    public void AddRequestHandlers<T>()
    {
        var assembly = typeof(T).Assembly;
        var handlerInterface = typeof(IRequestHandler<,>);

        var handlers = assembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .SelectMany(t => t.GetInterfaces(), (t, i) => new { Type = t, Interface = i })
            .Where(t => t.Interface.IsGenericType && t.Interface.GetGenericTypeDefinition() == handlerInterface);

        foreach (var handler in handlers)
        {
            Services.AddScoped(handler.Interface, handler.Type);
        }
    }

    public MediatorKitBuilder AddNotificationPipelineBehavior<TPipelineBehavior, TNotification>()
        where TPipelineBehavior : class, INotificationPipelineBehavior<TNotification>
        where TNotification : INotification
    {
        Services.AddScoped<INotificationPipelineBehavior<TNotification>, TPipelineBehavior>();
        return this;
    }

    public MediatorKitBuilder AddNotificationPipelineBehavior(Type behaviorType)
    {
        if (!behaviorType.IsGenericTypeDefinition)
        {
            throw new ArgumentException("Behavior must be an open generic type, e.g. typeof(LoggingBehavior<>)");
        }

        var isValidBehavior = !behaviorType.GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(INotificationPipelineBehavior<>));

        if (isValidBehavior)
        {
            throw new ArgumentException(
                "Behavior type must implement INotificationPipelineBehavior<> (Parameter 'behaviorType')");
        }

        Services.AddScoped(typeof(INotificationPipelineBehavior<>), behaviorType);
        return this;
    }

    public void AddNotificationHandlers<T>()
    {
        var assembly = typeof(T).Assembly;
        var handlerInterface = typeof(INotificationHandler<>);

        var handlers = assembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .SelectMany(t => t.GetInterfaces(), (t, i) => new { Type = t, Interface = i })
            .Where(t => t.Interface.IsGenericType && t.Interface.GetGenericTypeDefinition() == handlerInterface);

        foreach (var handler in handlers)
        {
            Services.AddScoped(handler.Interface, handler.Type);
        }
    }
}

public static class DependencyInjection
{
    public static void AddMediatorKit(this IServiceCollection services, Action<MediatorKitBuilder>? configure = null)
    {
        var builder = new MediatorKitBuilder(services);
        configure?.Invoke(builder);

        services.AddScoped<ISender, Sender>();
        services.AddScoped<IPublisher, Publisher>();
    }
}