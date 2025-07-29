namespace SimpleTransit;

/// <summary>
/// Defines a contract for publishing notifications asynchronously.
/// </summary>
/// <remarks>
/// <para>
/// This interface is designed to decouple the notification publishing mechanism from its implementation.
/// It allows for flexibility in how notifications are delivered, such as via messaging systems, event buses, or other means.
/// </para>
/// <para>
/// Notifications are processed immediately and synchronously by all registered <see cref="INotificationHandler{TMessage}"/>
/// implementations. This makes it suitable for scenarios requiring immediate processing, such as cache updates
/// or real-time notifications. The method returns only after all handlers have completed processing.
/// </para>
/// </remarks>
public interface INotificationPublisher
{
    /// <summary>
    /// Publishes a notification message and processes it immediately through all registered handlers before returning.
    /// </summary>
    /// <typeparam name="TMessage">The type of the message to be published.</typeparam>
    /// <param name="message">The notification message to be published. This could represent an event, command, or any other type of message. Cannot be <see langword="null"/>.</param>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/> that can be used to cancel the operation.
    /// </param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation. The task completes only after all notification handlers have finished processing.</returns>
    /// <remarks>
    /// <para>
    /// This method processes notifications immediately in the calling context by invoking all registered 
    /// <see cref="INotificationHandler{TMessage}"/> implementations sequentially. Each handler is awaited 
    /// individually, ensuring that all handlers complete before the method returns.
    /// </para>
    /// <para>
    /// If any handler throws an exception, the exception is propagated to the caller and subsequent 
    /// handlers may not be executed. This provides immediate feedback about processing failures.
    /// </para>
    /// </remarks>
    Task NotifyAsync<TMessage>(TMessage message, CancellationToken cancellationToken = default) where TMessage : notnull;
}
