using System.Threading.Channels;

namespace SimpleTransit.Queues;

internal class InMemoryMessageQueue : IMessageQueue
{
    private readonly Channel<IMessage> channel = Channel.CreateUnbounded<IMessage>();

    public IAsyncEnumerable<IMessage> ReadAllAsync(CancellationToken cancellationToken)
        => channel.Reader.ReadAllAsync(cancellationToken);

    public Task WriteAsync(IMessage message, CancellationToken cancellationToken)
        => channel.Writer.WriteAsync(message, cancellationToken).AsTask();
}
