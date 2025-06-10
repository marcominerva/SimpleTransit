using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SimpleTransit.Queues;

internal class MessageQueueProcessor(SimpleTransitScopeResolver scopeResolver, IMessageQueue queue, ILogger<MessageQueueProcessor> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var message in queue.ReadAllAsync(stoppingToken))
        {
            await using var scope = scopeResolver.CreateAsyncScope();

            var consumerType = typeof(IConsumer<>).MakeGenericType(message.GetType());
            var handlers = scope.ServiceProvider.GetServices(consumerType).ToList();

            if (handlers.Count == 0)
            {
                logger.LogWarning("No consumers found for message type {MessageType}", message.GetType().Name);
                continue;
            }

            foreach (var handler in handlers)
            {
                try
                {
                    await InvokeHandlerAsync(consumerType, handler!, message, stoppingToken);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Unexpected error while handling message of type {MessageType}", message.GetType().Name);
                }
            }
        }
    }

    private static async Task InvokeHandlerAsync(Type consumerType, object handler, IMessage message, CancellationToken cancellationToken)
    {
        // Use reflection to invoke the method on the IConsumer interface.
        var consumeMethod = consumerType.GetMethod(nameof(IConsumer<IMessage>.HandleAsync));
        if (consumeMethod is not null)
        {
            await (Task)consumeMethod.Invoke(handler, [message, cancellationToken])!;
        }
    }
}
