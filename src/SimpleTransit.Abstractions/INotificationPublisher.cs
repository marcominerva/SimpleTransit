namespace SimpleTransit;

/// <summary>
/// Defines a contract for publishing notifications asynchronously.
/// </summary>
/// <remarks>
/// This interface is designed to decouple the notification publishing mechanism from its implementation.
/// It allows for flexibility in how notifications are delivered, such as via messaging systems, event buses, or other means.
/// </remarks>
public interface INotificationPublisher
{
    /// <summary>
    /// Publishes a notification message asynchronously.
    /// </summary>
    /// <typeparam name="TMessage">The type of the message to be published.</typeparam>
    /// <param name="message">The notification message to be published. This could represent an event, command, or any other type of message.</param>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/> that can be used to cancel the operation.
    /// </param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    Task NotifyAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default) where TMessage : notnull;
}
