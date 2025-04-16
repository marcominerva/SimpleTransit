namespace SimpleTransit.Abstractions;

public interface IMessagePublisher
{
    Task PublishAsync<TMessage>(TMessage notification, CancellationToken cancellationToken = default) where TMessage : class, IMessage;
}
