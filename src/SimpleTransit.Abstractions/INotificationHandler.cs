namespace SimpleTransit.Abstractions;

public interface INotificationHandler<TMessage>
{
    Task HandleAsync(TMessage message, CancellationToken cancellationToken);
}