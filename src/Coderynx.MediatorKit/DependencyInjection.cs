using Coderynx.MediatorKit.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Coderynx.MediatorKit;

public class MediatorKitBuilder
{
    internal MediatorKitBuilder(IServiceCollection services)
    {
        Services = services;
    }

    internal IServiceCollection Services { get; }

    public MediatorKitBuilder AddPipelineBehavior<TPipelineBehavior, TRequest, TResponse>()
        where TPipelineBehavior : class, IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        Services.AddScoped<IPipelineBehavior<TRequest, TResponse>, TPipelineBehavior>();
        return this;
    }

    public MediatorKitBuilder AddPipelineBehavior(Type behaviorType)
    {
        if (!behaviorType.IsGenericTypeDefinition)
        {
            throw new ArgumentException("Behavior must be an open generic type, e.g. typeof(LoggingBehavior<,>)");
        }

        if (!behaviorType.GetInterfaces()
                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>)))
        {
            throw new ArgumentException("Behavior type must implement IPipelineBehavior<,> (Parameter 'behaviorType')");
        }

        Services.AddScoped(typeof(IPipelineBehavior<,>), behaviorType);
        return this;
    }

    public void AddHandlers<T>()
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
}

public static class DependencyInjection
{
    public static void AddMediatorKit(this IServiceCollection services, Action<MediatorKitBuilder>? configure = null)
    {
        var cqrsBuilder = new MediatorKitBuilder(services);
        configure?.Invoke(cqrsBuilder);

        services.AddScoped<ISender, Sender>();
    }
}