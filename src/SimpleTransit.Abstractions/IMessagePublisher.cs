namespace SimpleTransit;

/// <summary>
/// Defines a contract for publishing messages to a messaging system.
/// </summary>
/// <remarks>
/// This interface is designed to abstract the process of sending messages, allowing for flexibility in the underlying
/// implementation (e.g., message queues, event buses, etc.). It ensures that any class implementing this interface
/// can publish messages of a specific type that implements <see cref="IMessage"/>.
/// </remarks>
public interface IMessagePublisher
{
    /// <summary>
    /// Publishes a message asynchronously to the messaging system.
    /// </summary>
    /// <typeparam name="TMessage">The type of the message to be published. Must implement <see cref="IMessage"/>.</typeparam>
    /// <param name="message">The message instance to be published.</param>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/> to observe while waiting for the task to complete.
    /// </param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    /// <remarks>
    /// This method is intended to be used for decoupling components by enabling communication through messages.
    /// Implementations may include additional logic such as serialization, logging, or retry mechanisms.
    /// </remarks>
    Task PublishAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default) where TMessage : class, IMessage;
}
