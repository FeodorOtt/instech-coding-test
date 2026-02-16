using System.Threading.Channels;

namespace Claims.Auditing;

/// <summary>
/// An unbounded in-memory queue for audit messages, backed by <see cref="Channel{T}"/>.
/// </summary>
public interface IAuditQueue
{
    /// <summary>
    /// Enqueues an audit message for background processing.
    /// </summary>
    ValueTask EnqueueAsync(AuditMessage message);

    /// <summary>
    /// Reads audit messages as they become available. Used by the background processor.
    /// </summary>
    IAsyncEnumerable<AuditMessage> ReadAllAsync(CancellationToken cancellationToken);
}

/// <inheritdoc />
public class AuditQueue : IAuditQueue
{
    private readonly Channel<AuditMessage> _channel = Channel.CreateUnbounded<AuditMessage>(
        new UnboundedChannelOptions { SingleReader = true });

    /// <inheritdoc />
    public ValueTask EnqueueAsync(AuditMessage message) => _channel.Writer.WriteAsync(message);

    /// <inheritdoc />
    public IAsyncEnumerable<AuditMessage> ReadAllAsync(CancellationToken cancellationToken) =>
        _channel.Reader.ReadAllAsync(cancellationToken);
}
