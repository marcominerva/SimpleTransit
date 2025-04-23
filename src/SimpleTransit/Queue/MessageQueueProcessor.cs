using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SimpleTransit.Abstractions;

namespace SimpleTransit.Queue;

internal class MessageQueueProcessor(IServiceProvider serviceProvider, InMemoryMessageQueue queue, SimpleTransitOptions options, ILogger<MessageQueueProcessor> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var message in queue.ReadAllAsync(stoppingToken))
        {
            await using var scope = serviceProvider.CreateAsyncScope();

            var consumerType = typeof(IConsumer<>).MakeGenericType(message.GetType());
            var handlers = scope.ServiceProvider.GetServices(consumerType).ToList();

            if (handlers.Count == 0)
            {
                logger.LogWarning("No handlers found for message type {MessageType}", message.GetType().Name);
                continue;
            }

            var executionTask = options.ConsumerPublishStrategy switch
            {
                PublishStrategy.AwaitForEach => AwaitForEachAsync(consumerType, handlers!, message, stoppingToken),
                PublishStrategy.AwaitWhenAll => AwaitWhenAllAsync(consumerType, handlers!, message, stoppingToken),
                _ => throw new UnreachableException($"Publish strategy '{options.ConsumerPublishStrategy}' is not supported.")
            };

            await executionTask;
        }
    }

    private async Task AwaitForEachAsync(Type consumerType, List<object> handlers, IMessage message, CancellationToken cancellationToken)
    {
        foreach (var handler in handlers)
        {
            try
            {
                await InvokeHandlerAsync(consumerType, handler, message, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error while handling message of type {MessageType}", message.GetType().Name);
            }
        }
    }

    private async Task AwaitWhenAllAsync(Type consumerType, List<object> handlers, IMessage message, CancellationToken cancellationToken)
    {
        var tasks = handlers.Select(handler => InvokeHandlerAsync(consumerType, handler, message, cancellationToken));

        try
        {
            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error while handling message of type {MessageType}", message.GetType().Name);
        }
    }

    private static async Task InvokeHandlerAsync(Type consumerType, object handler, IMessage message, CancellationToken cancellationToken)
    {
        // Use reflection to invoke the method on the IConsumer interface.
        var consumeMethod = consumerType.GetMethod("HandleAsync");
        if (consumeMethod is not null)
        {
            await (Task)consumeMethod.Invoke(handler, [message, cancellationToken])!;
        }
    }
}
