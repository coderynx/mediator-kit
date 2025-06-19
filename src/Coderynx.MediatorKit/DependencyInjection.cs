using Coderynx.MediatorKit.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Coderynx.MediatorKit;

public class CqrsBuilder
{
    internal CqrsBuilder(IServiceCollection services)
    {
        Services = services;
    }

    internal IServiceCollection Services { get; }

    public CqrsBuilder AddPipelineBehavior<TPipelineBehavior, TRequest, TResponse>()
        where TPipelineBehavior : class, IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        Services.AddScoped<IPipelineBehavior<TRequest, TResponse>, TPipelineBehavior>();
        return this;
    }

    public CqrsBuilder AddPipelineBehavior(Type behaviorType)
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

    public void AddCommandHandlers<T>()
    {
        var asm = typeof(T).Assembly;

        var types = asm.GetTypes().Where(t => t is { IsClass: true, IsAbstract: false });

        foreach (var type in types)
        {
            var interfaces = type.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommandHandler<,>));

            foreach (var @interface in interfaces)
            {
                Services.AddScoped(@interface, type);
            }
        }
    }

    public void AddQueryHandlers<T>()
    {
        var asm = typeof(T).Assembly;

        var types = asm.GetTypes().Where(t => t is { IsClass: true, IsAbstract: false });

        foreach (var type in types)
        {
            var interfaces = type.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQueryHandler<,>));

            foreach (var @interface in interfaces)
            {
                Services.AddScoped(@interface, type);
            }
        }
    }
}

public static class DependencyInjection
{
    public static void AddCqrs(this IHostApplicationBuilder builder, Action<CqrsBuilder>? configure = null)
    {
        var cqrsBuilder = new CqrsBuilder(builder.Services);
        configure?.Invoke(cqrsBuilder);

        builder.Services.AddScoped<ISender, Sender>();
    }
}