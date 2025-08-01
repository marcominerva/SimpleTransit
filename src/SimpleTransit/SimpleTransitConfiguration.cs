﻿using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace SimpleTransit;

/// <summary>
/// Provides configuration options for the SimpleTransit library, enabling the registration
/// of services such as notification handlers and message consumers. This class is designed
/// to facilitate the setup of message-based communication patterns in an application.
/// </summary>
public class SimpleTransitConfiguration
{
    private readonly IServiceCollection services;

    internal SimpleTransitConfiguration(IServiceCollection services)
    {
        this.services = services;
    }

    /// <summary>
    /// Indicates whether the configuration contains Notification Handlers. This is necessary because the notification publisher interface
    /// needs to be registered only if there are actual handlers. Notification handlers are responsible for processing notifications
    /// published by the system.
    /// </summary>
    internal bool ContainsNotificationHandlers { get; set; }

    /// <summary>
    /// Indicates whether the configuration contains Message Consumers. This is necessary because the message queue hosted service
    /// needs to be registered only if there are actual consumers. Message consumers are responsible for processing messages
    /// from a message queue.
    /// </summary>
    internal bool ContainsMessageConsumers { get; set; }

    /// <summary>
    /// Registers services from the assembly containing the specified type <typeparamref name="T"/>.
    /// This method scans the assembly for types implementing <see cref="INotificationHandler{T}"/> or <see cref="IConsumer{T}"/>
    /// and registers them with the dependency injection container.
    /// </summary>
    /// <typeparam name="T">A type from the target assembly.</typeparam>
    /// <param name="predicate">An optional predicate to filter the types to be registered.</param>
    /// <returns>The current <see cref="SimpleTransitConfiguration"/> instance for method chaining.</returns>
    public SimpleTransitConfiguration RegisterServicesFromAssemblyContaining<T>(Func<Type, bool>? predicate = null)
        => RegisterServicesFromAssembly(typeof(T).Assembly, predicate);

    /// <summary>
    /// Registers services from the specified assembly. This method scans the assembly for types implementing
    /// <see cref="INotificationHandler{T}"/> or <see cref="IConsumer{T}"/> and registers them with the dependency injection container.
    /// </summary>
    /// <param name="assembly">The assembly to scan for services.</param>
    /// <param name="predicate">An optional predicate to filter the types to be registered.</param>
    /// <returns>The current <see cref="SimpleTransitConfiguration"/> instance for method chaining.</returns>
    public SimpleTransitConfiguration RegisterServicesFromAssembly(Assembly assembly, Func<Type, bool>? predicate = null)
        => RegisterServicesFromAssemblies([assembly], predicate);

    /// <summary>
    /// Registers services from the specified assemblies. This method scans the assemblies for types implementing
    /// <see cref="INotificationHandler{T}"/> or <see cref="IConsumer{T}"/> and registers them with the dependency injection container.
    /// </summary>
    /// <param name="assemblies">The assemblies to scan for services.</param>
    /// <param name="predicate">An optional predicate to filter the types to be registered.</param>
    /// <returns>The current <see cref="SimpleTransitConfiguration"/> instance for method chaining.</returns>
    public SimpleTransitConfiguration RegisterServicesFromAssemblies(IEnumerable<Assembly> assemblies, Func<Type, bool>? predicate = null)
    {
        var notificationHandlerInterfaceType = typeof(INotificationHandler<>);
        var notificationHandlersToRegister = GetTypes(assemblies, notificationHandlerInterfaceType, predicate);

        if (notificationHandlersToRegister.Count > 0)
        {
            ContainsNotificationHandlers = true;
            RegisterTypes(services, notificationHandlersToRegister, notificationHandlerInterfaceType);
        }

        var consumerInterfaceType = typeof(IConsumer<>);
        var consumersToRegister = GetTypes(assemblies, consumerInterfaceType, predicate);

        if (consumersToRegister.Count > 0)
        {
            ContainsMessageConsumers = true;
            RegisterTypes(services, consumersToRegister, consumerInterfaceType);
        }

        return this;

        static IList<Type> GetTypes(IEnumerable<Assembly> assemblies, Type interfaceType, Func<Type, bool>? predicate = null)
        {
            var typesToRegister = assemblies
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsClass && !type.IsAbstract)
                .Where(type => type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType))
                .Where(type => predicate is null || predicate(type))
                .ToList();

            return typesToRegister;
        }

        static void RegisterTypes(IServiceCollection services, IEnumerable<Type> typesToRegister, Type interfaceType)
        {
            foreach (var type in typesToRegister)
            {
                var interfaces = type.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == interfaceType).ToList();

                foreach (var @interface in interfaces)
                {
                    services.AddTransient(@interface, type);
                }
            }
        }
    }

    /// <summary>
    /// Marks the configuration as containing notification handlers. This is useful when handlers
    /// are registered manually or through other means outside of this configuration class.
    /// </summary>
    /// <returns>The current <see cref="SimpleTransitConfiguration"/> instance for method chaining.</returns>
    public SimpleTransitConfiguration UseNotificationHandlers()
    {
        ContainsNotificationHandlers = true;
        return this;
    }

    /// <summary>
    /// Marks the configuration as containing message consumers. This is useful when consumers
    /// are registered manually or through other means outside of this configuration class.
    /// </summary>
    /// <returns>The current <see cref="SimpleTransitConfiguration"/> instance for method chaining.</returns>
    public SimpleTransitConfiguration UseMessageConsumers()
    {
        ContainsMessageConsumers = true;
        return this;
    }
}
