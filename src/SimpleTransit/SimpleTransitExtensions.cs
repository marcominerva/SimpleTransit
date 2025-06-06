using Microsoft.Extensions.DependencyInjection;
using SimpleTransit.Queues;

namespace SimpleTransit;

/// <summary>
/// Provides extension methods for configuring and registering SimpleTransit services in an <see cref="IServiceCollection"/>.
/// </summary>
public static class SimpleTransitExtensions
{
    /// <summary>
    /// Adds and configures SimpleTransit services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to which the services will be added.</param>
    /// <param name="configure">
    /// A delegate to configure the <see cref="SimpleTransitConfiguration"/>. This allows the caller to specify
    /// how notification handlers and message consumers should be registered and how messages should be published.
    /// </param>
    /// <returns>The updated <see cref="IServiceCollection"/> with SimpleTransit services registered.</returns>
    public static IServiceCollection AddSimpleTransit(this IServiceCollection services, Action<SimpleTransitConfiguration> configure)
    {
        var configuration = new SimpleTransitConfiguration(services);
        configure.Invoke(configuration);

        if (configuration.ContainsNotificationHandlers || configuration.ContainsMessageConsumers)
        {
            // Register core SimpleTransit services.
            services.AddSingleton<SimpleTransitScopeResolver>();
            services.AddSingleton<SimpleTransit>();
        }

        if (configuration.ContainsNotificationHandlers)
        {
            // Register INotificationPublisher interface only if there are actual handlers.
            services.AddHttpContextAccessor();

            services.AddSingleton<INotificationPublisher>(services => services.GetRequiredService<SimpleTransit>());
        }

        if (configuration.ContainsMessageConsumers)
        {
            // Register IMessagePublisher interface and related services only if there are actual consumers.
            services.AddSingleton<IMessageQueue, InMemoryMessageQueue>();
            services.AddHostedService<MessageQueueProcessor>();

            services.AddSingleton<IMessagePublisher>(services => services.GetRequiredService<SimpleTransit>());
        }

        return services;
    }
}
