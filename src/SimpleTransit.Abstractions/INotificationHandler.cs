namespace SimpleTransit.Abstractions;

public interface INotificationHandler<TMessage>
{
    Task HandleAsync(TMessage notification, CancellationToken cancellationToken = default);
}