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

    public ChannelReader<IMessage> Reader => channel.Reader;

    public ChannelWriter<IMessage> Writer => channel.Writer;
}
