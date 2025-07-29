using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SimpleTransit;

/// <summary>
/// The core implementation of SimpleTransit that provides both notification publishing and message publishing capabilities.
/// </summary>
/// <remarks>
/// <para>
/// This class serves as the central hub for message-based communication within the SimpleTransit system.
/// It implements both <see cref="INotificationPublisher"/> for immediate, synchronous notification handling
/// and <see cref="IMessagePublisher"/> for asynchronous, queued message processing.
/// </para>
/// <para>
/// The implementation automatically resolves appropriate handlers and consumers using dependency injection,
/// ensuring proper scoping and lifecycle management of services.
/// </para>
/// </remarks>
internal partial class SimpleTransit(SimpleTransitScopeResolver scopeResolver, ILogger<SimpleTransit> logger, IMessageQueue? queue = null) : INotificationPublisher, IMessagePublisher
{
    private readonly ILogger<SimpleTransit> logger = logger;

    /// <inheritdoc />
    public async Task NotifyAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default) where TMessage : notnull
    {
        ArgumentNullException.ThrowIfNull(message);

        (var serviceProvider, var isOwned) = scopeResolver.GetOrCreate();

        try
        {
            var messageType = typeof(TMessage).Name;

            var handlers = serviceProvider.GetServices<INotificationHandler<TMessage>>().ToList();
            if (handlers.Count == 0)
            {
                LogNoHandlersFound(messageType);
                return;
            }

            foreach (var handler in handlers)
            {
                try
                {
                    await handler.HandleAsync(message, cancellationToken);
                }
                catch (Exception ex)
                {
                    LogNotificationHandlingError(ex, messageType, handler.GetType().Name);

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

    /// <inheritdoc />
    public async Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default) where TMessage : class, IMessage
    {
        ArgumentNullException.ThrowIfNull(message);

        if (queue is null)
        {
            throw new InvalidOperationException("Message queue not available: no consumers defined");
        }

        await queue.WriteAsync(message, cancellationToken);
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Warning, Message = "No handlers found for message type {messageType}")]
    partial void LogNoHandlersFound(string messageType);

    [LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "Unexpected error while handling notification for type {messageType} with handler {handlerType}")]
    partial void LogNotificationHandlingError(Exception exception, string messageType, string handlerType);
}
