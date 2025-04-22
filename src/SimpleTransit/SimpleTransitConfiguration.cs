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

    /// <summary>
    /// Indicates whether the configuration contains Notification Handlers. This is necessary because the notification publisher interface
    /// need to be registered only if there are actual handlers.
    /// </summary>
    internal bool ContainsNotificationHandlers { get; set; }

    /// <summary>
    /// Indicates whether the configuration contains Message Consumers. This is necessary because the message queue hosted service
    /// need to be registered only if there are actual consumers.
    /// </summary>
    internal bool ContainsMessageConsumers { get; set; }

    public PublishStrategy NotificationPublishStrategy { get; set; } = PublishStrategy.AwaitForEach;

    public PublishStrategy ConsumerPublishStrategy { get; set; } = PublishStrategy.AwaitForEach;

    public SimpleTransitConfiguration RegisterServicesFromAssemblyContaining<T>()
        => RegisterServicesFromAssembly(typeof(T).Assembly);

    public SimpleTransitConfiguration RegisterServicesFromAssembly(Assembly assembly, Func<Type, bool>? predicate = null)
    {
        var notificationHandlerInterfaceType = typeof(INotificationHandler<>);

        var notificationHandlersToRegister = assembly.GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract)
            .Where(type => type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == notificationHandlerInterfaceType))
            .Where(type => predicate == null || predicate(type)).ToList();

        if (notificationHandlersToRegister.Count > 0)
        {
            ContainsNotificationHandlers = true;
            RegisterTypes(services, notificationHandlersToRegister, notificationHandlerInterfaceType);
        }

        var consumerInterfaceType = typeof(IConsumer<>);

        var consumersToRegister = assembly.GetTypes()
            .Where(type => type.IsClass && !type.IsAbstract)
            .Where(type => type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == consumerInterfaceType))
            .Where(type => predicate == null || predicate(type)).ToList();

        if (consumersToRegister.Count > 0)
        {
            ContainsMessageConsumers = true;
            RegisterTypes(services, consumersToRegister, consumerInterfaceType);
        }

        return this;

        static void RegisterTypes(IServiceCollection services, IEnumerable<Type> typesToRegister, Type interfaceType)
        {
            foreach (var type in typesToRegister)
            {
                var interfaces = type.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType);

                foreach (var @interface in interfaces)
                {
                    services.AddTransient(@interface, type);
                }
            }
        }
    }

    public SimpleTransitConfiguration UseNotificationHandlers()
    {
        ContainsNotificationHandlers = true;
        return this;
    }

    public SimpleTransitConfiguration UseMessageConsumers()
    {
        ContainsMessageConsumers = true;
        return this;
    }
}
