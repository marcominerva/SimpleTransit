namespace SimpleTransit.Abstractions;

public interface INotificationPublisher
{
    Task NotifyAsync<TMessage>(TMessage notification, CancellationToken cancellationToken = default);
}
