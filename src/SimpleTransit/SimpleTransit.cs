using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SimpleTransit.Abstractions;
using SimpleTransit.Queue;

namespace SimpleTransit;

internal class SimpleTransit(IServiceProvider serviceProvider, SimpleTransitOptions options, ILogger<SimpleTransit> logger, InMemoryMessageQueue? queue = null) : INotificationPublisher, IMessagePublisher
{
    public async Task NotifyAsync<TMessage>(TMessage notification, CancellationToken cancellationToken = default)
    {
        var handlers = serviceProvider.GetServices<INotificationHandler<TMessage>>().ToList();
        if (handlers.Count == 0)
        {
            return;
        }

        var executionTask = options.NotificationPublishStrategy switch
        {
            PublishStrategy.AwaitForEach => AwaitForEachAsync(notification, handlers, cancellationToken),
            PublishStrategy.AwaitWhenAll => AwaitWhenAllAsync(notification, handlers, cancellationToken),
            _ => throw new UnreachableException($"Publish strategy '{options.NotificationPublishStrategy}' is not supported.")
        };

        await executionTask;
    }

    public async Task PublishAsync<TMessage>(TMessage notification, CancellationToken cancellationToken = default) where TMessage : class, IMessage
    {
        if (queue is null)
        {
            throw new InvalidOperationException("Message queue not available: no consumers defined");
        }

        await queue.Writer.WriteAsync(notification, cancellationToken);
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
                logger.LogError(ex, "Error handling notification");

                // Rethrow the exception to the caller.
                throw;
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
