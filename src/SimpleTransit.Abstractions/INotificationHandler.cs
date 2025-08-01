﻿namespace SimpleTransit;

/// <summary>
/// Defines a contract for handling notifications of a specific type.
/// </summary>
/// <typeparam name="TMessage">
/// The type of the message that this handler processes. This allows for strongly-typed handling of notifications.
/// </typeparam>
/// <remarks>
/// <para>
/// This interface is designed to be implemented by classes that handle notifications synchronously and immediately
/// when they are published. Unlike <see cref="IConsumer{TMessage}"/>, which processes messages asynchronously
/// through a queue, notification handlers execute directly in the calling context.
/// </para>
/// <para>
/// Notification handlers are suitable for scenarios where immediate processing is required, such as updating
/// caches, sending real-time notifications, or performing validation logic.
/// </para>
/// </remarks>
public interface INotificationHandler<TMessage>
{
    /// <summary>
    /// Asynchronously handles a notification message of type <typeparamref name="TMessage"/>.
    /// </summary>
    /// <param name="message">
    /// The notification message to be processed. This is the core data that the handler will act upon.
    /// Cannot be <see langword="null"/>.
    /// </param>
    /// <param name="cancellationToken">
    /// A <see cref="CancellationToken"/> that can be used to cancel the operation if needed.
    /// </param>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation.
    /// </returns>
    Task HandleAsync(TMessage message, CancellationToken cancellationToken);
}
