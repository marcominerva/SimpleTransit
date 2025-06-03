namespace SimpleTransit;

/// <summary>
/// Defines the contract for a message queue abstraction, enabling asynchronous, decoupled communication
/// between producers and consumers in distributed or modular systems.
/// <para>
/// The intent of this interface is to provide a minimal, technology-agnostic API for reading and writing
/// messages, so that implementations can target various underlying queueing technologies (e.g., in-memory,
/// cloud queues, message brokers) without affecting application logic.
/// </para>
/// <para>
/// This abstraction is crucial for enabling testability, scalability, and flexibility in message-driven architectures.
/// </para>
/// </summary>
public interface IMessageQueue
{
    /// <summary>
    /// Asynchronously reads all available messages from the queue as an <see cref="IAsyncEnumerable{T}"/> of <see cref="IMessage"/>.
    /// <para>
    /// This method is intended to be used by consumers to process messages as they arrive, supporting efficient streaming
    /// and backpressure-aware consumption patterns.
    /// </para>
    /// </summary>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/> that can be used to cancel the asynchronous read operation.
    /// </param>
    /// <returns>
    /// An <see cref="IAsyncEnumerable{IMessage}"/> representing the stream of messages available for consumption.
    /// </returns>
    IAsyncEnumerable<IMessage> ReadAllAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Asynchronously writes a message to the queue for later consumption.
    /// <para>
    /// This method is intended to be used by producers to enqueue messages, decoupling the sender from the receiver
    /// and enabling reliable, asynchronous communication.
    /// </para>
    /// </summary>
    /// <param name="message">
    /// The <see cref="IMessage"/> instance to enqueue.
    /// </param>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/> that can be used to cancel the asynchronous write operation.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous write operation.
    /// </returns>
    Task WriteAsync(IMessage message, CancellationToken cancellationToken);
}
