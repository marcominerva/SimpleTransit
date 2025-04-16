namespace SimpleTransit.Abstractions;

public interface IConsumer<TMessage> where TMessage : class, IMessage
{
    Task HandleAsync(TMessage notification, CancellationToken cancellationToken);
}