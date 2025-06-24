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
        if (!behaviorType.IsGenericType)
        {
            throw new ArgumentException("Behavior type must be generic", nameof(behaviorType));
        }

        var genericTypeDefinition = behaviorType.GetGenericTypeDefinition();
        var interfaceType = typeof(IPipelineBehavior<,>);

        var behaviorTypes = !genericTypeDefinition.GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType);

        if (behaviorTypes)
        {
            throw new ArgumentException("Behavior type must implement IPipelineBehavior<,>", nameof(behaviorType));
        }

        Services.AddScoped(interfaceType, behaviorType);
        return this;
    }

    public void AddHandlers<T>()
    {
        var assembly = typeof(T).Assembly;
        var types = assembly.GetTypes().Where(t => t is { IsClass: true, IsAbstract: false });

        foreach (var type in types)
        {
            var interfaces = type.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>));

            foreach (var @interface in interfaces)
            {
                Services.AddScoped(@interface, type);
            }
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