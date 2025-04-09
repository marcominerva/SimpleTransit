using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SimpleTransit.Abstractions;

namespace SimpleTransit;

public static class SimpleTransitExtensions
{
    public static IServiceCollection AddSimpleTransit(this IServiceCollection services, Action<SimpleTransitConfiguration> configurationAction)
    {
        services.TryAddSingleton<INotificator, SimpleTransit>();

        var configuration = new SimpleTransitConfiguration(services);
        configurationAction.Invoke(configuration);

        services.AddSingleton(new SimpleTransitOptions
        {
            UnhandledExceptionBehavior = configuration.UnhandledExceptionBehavior
        });

        return services;
    }
}
