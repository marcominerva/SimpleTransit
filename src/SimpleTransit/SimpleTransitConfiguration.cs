using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using SimpleTransit.Abstractions;

namespace SimpleTransit;

public class SimpleTransitConfiguration
{
    private readonly IServiceCollection services;

    internal SimpleTransitConfiguration(IServiceCollection services)
    {
        this.services = services;
    }

    public UnhandledExceptionBehavior UnhandledExceptionBehavior { get; set; } = UnhandledExceptionBehavior.Throw;

    public PublishStrategy PublishStrategy { get; set; } = PublishStrategy.AwaitForEach;

    public SimpleTransitConfiguration RegisterServiceFromAssemblyContaining<T>()
        => RegisterServiceFromAssembly(typeof(T).Assembly);

    public SimpleTransitConfiguration RegisterServiceFromAssembly(Assembly assembly, Func<Type, bool>? predicate = null)
    {
        var notificationHandlerInterfaceType = typeof(INotificationHandler<>);

        var typesToRegister = assembly.GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract)
            .Where(type => type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == notificationHandlerInterfaceType))
            .Where(type => predicate == null || predicate(type));

        foreach (var type in typesToRegister)
        {
            var interfaces = type.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == notificationHandlerInterfaceType);

            foreach (var @interface in interfaces)
            {
                services.AddTransient(@interface, type);
            }
        }

        return this;
    }
}
