using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SimpleTransit;

internal class SimpleTransit(SimpleTransitScopeResolver scopeResolver, ILogger<SimpleTransit> logger, IMessageQueue? queue = null) : INotificationPublisher, IMessagePublisher
{
    public async Task NotifyAsync<TMessage>(TMessage notification, CancellationToken cancellationToken = default) where TMessage : notnull
    {
        (var serviceProvider, var isOwned) = scopeResolver.GetOrCreate();

        try
        {
            var handlers = serviceProvider.GetServices<INotificationHandler<TMessage>>().ToList();
            if (handlers.Count == 0)
            {
                logger.LogWarning("No handlers found for message type {MessageType}", typeof(TMessage).Name);
                return;
            }

            foreach (var handler in handlers)
            {
                try
                {
                    await handler.HandleAsync(notification, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Unexpected error while handling notification of type {MessageType}", typeof(TMessage).Name);

                    // Rethrow the exception to the caller.
                    throw;
                }
            }
        }
        finally
        {
            if (isOwned)
            {
                await ((IAsyncDisposable)serviceProvider).DisposeAsync();
            }
        }
    }

    public async Task PublishAsync<TMessage>(TMessage notification, CancellationToken cancellationToken = default) where TMessage : class, IMessage
    {
        if (queue is null)
        {
            throw new InvalidOperationException("Message queue not available: no consumers defined");
        }

        await queue.WriteAsync(notification, cancellationToken);
    }
}
