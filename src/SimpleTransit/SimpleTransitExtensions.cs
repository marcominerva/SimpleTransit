using Microsoft.Extensions.DependencyInjection;
using SimpleTransit.Abstractions;

namespace SimpleTransit;

public static class SimpleTransitExtensions
{
    public static IServiceCollection AddSimpleTransit(this IServiceCollection services, Action<SimpleTransitConfiguration> configure)
    {
        services.AddScoped<SimpleTransit>();
        services.AddScoped<INotificationPublisher>(services => services.GetRequiredService<SimpleTransit>());

        var configuration = new SimpleTransitConfiguration(services);
        configure.Invoke(configuration);

        services.AddSingleton(new SimpleTransitOptions
        {
            PublishStrategy = configuration.PublishStrategy
        });

        return services;
    }
}
