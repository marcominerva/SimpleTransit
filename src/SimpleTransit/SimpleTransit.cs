using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleTransit.Abstractions;

namespace SimpleTransit;

internal class SimpleTransit(IServiceProvider serviceProvider, SimpleTransitOptions options, ILogger<SimpleTransit> logger) : INotificator
{
    public async Task NotifyAsync<TMessage>(TMessage notification, CancellationToken cancellationToken = default)
    {
        var handlers = serviceProvider.GetServices<INotificationHandler<TMessage>>().ToList();

        foreach (var handler in handlers)
        {
            try
            {
                await handler.HandleAsync(notification, cancellationToken);
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                logger.LogError(ex, "Error handling notification");

                if (options.UnhandledExceptionBehavior == UnhandledExceptionBehavior.Throw)
                {
                    // If the behavior is to stop on unhandled exceptions, rethrow the exception
                    throw;
                }
            }
        }

        //if (handlers.Count == 0)
        //    return Task.CompletedTask;

        //var tasks = handlers.Select(handler => handler.HandleAsync(notification, cancellationToken));
        //return Task.WhenAll(tasks);
    }
}
