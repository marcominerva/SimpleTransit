using Microsoft.Extensions.DependencyInjection;
using SimpleTransit.Abstractions;
using SimpleTransit.Queue;

namespace SimpleTransit;

public static class SimpleTransitExtensions
{
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
            services.AddScoped<SimpleTransit>();
            services.AddSingleton(new SimpleTransitOptions
            {
                NotificationPublishStrategy = configuration.PublishStrategy
            });
        }

        return services;
    }
}
