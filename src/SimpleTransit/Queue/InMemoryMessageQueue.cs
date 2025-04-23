using System.Threading.Channels;
using SimpleTransit.Abstractions;

namespace SimpleTransit.Queue;

internal class InMemoryMessageQueue
{
    private readonly Channel<IMessage> channel = Channel.CreateUnbounded<IMessage>(new UnboundedChannelOptions
    {
        SingleReader = true,
        SingleWriter = false
    });

    public IAsyncEnumerable<IMessage> ReadAllAsync(CancellationToken cancellationToken)
        => channel.Reader.ReadAllAsync(cancellationToken);

    public ValueTask WriteAsync(IMessage message, CancellationToken cancellationToken)
        => channel.Writer.WriteAsync(message, cancellationToken);
}
