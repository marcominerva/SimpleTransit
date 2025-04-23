using Microsoft.Extensions.DependencyInjection;
using SimpleTransit.Queue;

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

        if (configuration.ContainsNotificationHandlers)
        {
            // Register INotificationPublisher interface only if there are actual handlers.
            services.AddScoped<INotificationPublisher>(services => services.GetRequiredService<SimpleTransit>());
        }

        if (configuration.ContainsMessageConsumers)
        {
            // Register IMessagePublisher interface and related services only if there are actual consumers.
            services.AddSingleton<InMemoryMessageQueue>();
            services.AddHostedService<MessageQueueProcessor>();
            services.AddScoped<IMessagePublisher>(services => services.GetRequiredService<SimpleTransit>());
        }

        if (configuration.ContainsNotificationHandlers || configuration.ContainsMessageConsumers)
        {
            // Register core SimpleTransit service and its options.
            services.AddScoped<SimpleTransit>();
            services.AddSingleton(new SimpleTransitOptions
            {
                NotificationPublishStrategy = configuration.NotificationPublishStrategy,
                ConsumerPublishStrategy = configuration.ConsumerPublishStrategy
            });
        }

        return services;
    }
}
