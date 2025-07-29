using System.Threading.Channels;

namespace SimpleTransit.Queues;

/// <summary>
/// Provides an in-memory implementation of <see cref="IMessageQueue"/> using .NET channels
/// for high-performance, thread-safe message queueing.
/// </summary>
/// <remarks>
/// <para>
/// This implementation uses an unbounded channel optimized for single-reader, multiple-writer scenarios,
/// making it suitable for applications where messages are produced by multiple sources but consumed
/// by a single message processor.
/// </para>
/// <para>
/// The queue operates entirely in memory and does not provide persistence across application restarts.
/// For production scenarios requiring durability, consider implementing <see cref="IMessageQueue"/>
/// with a persistent message broker or database-backed solution.
/// </para>
/// </remarks>
internal class InMemoryMessageQueue : IMessageQueue, IAsyncDisposable
{
    private readonly Channel<IMessage> channel = Channel.CreateUnbounded<IMessage>(new()
    {
        SingleReader = true,
        SingleWriter = false
    });

    /// <inheritdoc />
    public IAsyncEnumerable<IMessage> ReadAllAsync(CancellationToken cancellationToken)
        => channel.Reader.ReadAllAsync(cancellationToken);

    /// <inheritdoc />
    public Task WriteAsync(IMessage message, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(message);

        return channel.Writer.WriteAsync(message, cancellationToken).AsTask();
    }

    /// <summary>
    /// Disposes the message queue by completing the writer and waiting for all readers to finish.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        channel.Writer.Complete();
        await channel.Reader.Completion;
    }
}