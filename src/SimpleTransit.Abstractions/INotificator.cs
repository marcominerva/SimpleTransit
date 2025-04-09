namespace SimpleTransit.Abstractions;

public interface INotificator
{
    Task NotifyAsync<TMessage>(TMessage notification, CancellationToken cancellationToken = default);
}
