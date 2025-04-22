using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SimpleTransit.Abstractions;

namespace SimpleTransit.Queue;

internal class MessageQueueProcessor(IServiceProvider serviceProvider, InMemoryMessageQueue queue, ILogger<MessageQueueProcessor> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var message in queue.Reader.ReadAllAsync(stoppingToken))
        {
            await using var scope = serviceProvider.CreateAsyncScope();

            var consumerType = typeof(IConsumer<>).MakeGenericType(message.GetType());
            var handlers = scope.ServiceProvider.GetServices(consumerType);

            foreach (var handler in handlers)
            {
                try
                {
                    // Use reflection to invoke the method on the IConsumer interface.
                    var consumeMethod = consumerType.GetMethod("HandleAsync");
                    if (consumeMethod is not null)
                    {
                        await (Task)consumeMethod.Invoke(handler, [message, stoppingToken])!;
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error handling message of type {MessageType}", message.GetType().Name);
                }
            }
        }
    }
}
