using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleTransit.Abstractions;

namespace SimpleTransit;

internal class SimpleTransit(IServiceProvider serviceProvider, SimpleTransitOptions options, ILogger<SimpleTransit> logger) : INotificator
{
    public async Task NotifyAsync<TMessage>(TMessage notification, CancellationToken cancellationToken = default)
    {
        var handlers = serviceProvider.GetServices<INotificationHandler<TMessage>>().ToList();
        if (handlers.Count == 0)
        {
            return;
        }

        if (options.PublishStrategy == PublishStrategy.AwaitForEach)
        {
            await AwaitForEachAsync(notification, handlers, cancellationToken);
        }
        else if (options.PublishStrategy == PublishStrategy.AwaitWhenAll)
        {
            if (options.UnhandledExceptionBehavior == UnhandledExceptionBehavior.Continue)
            {
                // When using AwaitWhenAll, we cannot throw exceptions if an exception occurs in one of the handlers,
                // because it will not be caught until all tasks are completed, so the Throw options will be ignored.
                logger.LogWarning("The 'AwaitWhenAll' PublishStrategy cannot be used when UnhandledExceptionBehavior is set to 'Throw', so the current UnhandledExceptionBehavior will be ignored");
            }

            await AwaitWhenAllAsync(notification, handlers, cancellationToken);
        }
    }

    private async Task AwaitForEachAsync<TMessage>(TMessage notification, List<INotificationHandler<TMessage>> handlers, CancellationToken cancellationToken)
    {
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
                    // If the behavior is to stop on unhandled exceptions, rethrow the exception.
                    // Otherwise, continue with the next handler.
                    throw;
                }
            }
        }
    }

    private async Task AwaitWhenAllAsync<TMessage>(TMessage notification, List<INotificationHandler<TMessage>> handlers, CancellationToken cancellationToken)
    {
        var tasks = handlers.Select(handler => handler.HandleAsync(notification, cancellationToken));

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling notification");

            // Rethrow the exception to the caller.
            throw;
        }
    }
}
