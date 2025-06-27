namespace SimpleTransit;

/// <summary>
/// Represents a contract for a consumer that processes messages of a specific type.
/// </summary>
/// <typeparam name="TMessage">
/// The type of the message to be consumed. Must implement the <see cref="IMessage"/> interface.
/// </typeparam>
/// <remarks>
/// <para>
/// This interface is designed to be implemented by classes that handle specific message types in an asynchronous manner.
/// Consumers are automatically discovered and registered by the SimpleTransit configuration system and are invoked
/// when messages of the corresponding type are published to the message queue.
/// </para>
/// <para>
/// Unlike <see cref="INotificationHandler{TMessage}"/>, which processes notifications synchronously,
/// consumers process messages asynchronously through a message queue, providing better decoupling and fault tolerance.
/// </para>
/// </remarks>
public interface IConsumer<TMessage> where TMessage : class, IMessage
{
    /// <summary>
    /// Handles the processing of a message asynchronously.
    /// </summary>
    /// <param name="message">The message to be processed. Cannot be <see langword="null"/>.</param>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/> that can be used to cancel the message processing operation.
    /// </param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task HandleAsync(TMessage message, CancellationToken cancellationToken);
}
