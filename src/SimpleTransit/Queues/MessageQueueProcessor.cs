using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SimpleTransit.Queues;

/// <summary>
/// A background service that continuously processes messages from an <see cref="IMessageQueue"/> by dispatching them
/// to their corresponding <see cref="IConsumer{TMessage}"/> implementations.
/// </summary>
/// <remarks>
/// <para>
/// This processor reads messages from the queue asynchronously and processes each message concurrently
/// without blocking the reading of subsequent messages. Each message is processed in its own task,
/// allowing for high throughput and efficient resource utilization.
/// </para>
/// <para>
/// The processor automatically resolves the appropriate consumers for each message type using dependency injection
/// and handles any exceptions that occur during message processing to ensure the service remains stable.
/// </para>
/// </remarks>
internal class MessageQueueProcessor(SimpleTransitScopeResolver scopeResolver, IMessageQueue queue, ILogger<MessageQueueProcessor> logger) : BackgroundService
{
    /// <summary>
    /// Executes the message processing loop, continuously reading messages from the queue
    /// and dispatching them for concurrent processing.
    /// </summary>
    /// <param name="stoppingToken">A <see cref="CancellationToken"/> that signals when the service should stop processing.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var message in queue.ReadAllAsync(stoppingToken))
        {
            // Process each message concurrently without waiting for completion.
            _ = Task.Run(async () =>
            {
                await ProcessMessageAsync(message, stoppingToken);
            }, stoppingToken);
        }
    }

    /// <summary>
    /// Processes a single message by resolving its consumers and invoking their handlers.
    /// </summary>
    /// <param name="message">The message to process.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while processing the message.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous processing operation.</returns>
    private async Task ProcessMessageAsync(IMessage message, CancellationToken cancellationToken)
    {
        await using var scope = scopeResolver.CreateAsyncScope();

        var messageType = message.GetType();
        var consumerType = typeof(IConsumer<>).MakeGenericType(messageType);
        var handlers = scope.ServiceProvider.GetServices(consumerType).ToList();

        if (handlers.Count == 0)
        {
            logger.LogWarning("No consumers found for message type {MessageType}", messageType.Name);
            return;
        }

        foreach (var handler in handlers)
        {
            try
            {
                await InvokeHandlerAsync(consumerType, handler!, message, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error while handling message for type {MessageType} with handler {HandlerType}", messageType.Name, handler!.GetType().Name);
            }
        }
    }

    /// <summary>
    /// Invokes the <see cref="IConsumer{TMessage}.HandleAsync"/> method on a consumer handler using reflection.
    /// </summary>
    /// <param name="consumerType">The generic consumer interface type for the message.</param>
    /// <param name="handler">The consumer handler instance to invoke.</param>
    /// <param name="message">The message to pass to the handler.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to pass to the handler.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous handler invocation.</returns>
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
