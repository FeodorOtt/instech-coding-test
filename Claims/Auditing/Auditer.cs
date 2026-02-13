namespace Claims.Auditing;

/// <summary>
/// Enqueues audit records onto an in-memory <see cref="IAuditQueue"/> for
/// background persistence. This returns almost immediately and never blocks
/// the calling HTTP request with a database write.
/// </summary>
public class Auditer : IAuditer
{
    private readonly IAuditQueue _queue;

    public Auditer(IAuditQueue queue)
    {
        _queue = queue;
    }

    /// <inheritdoc />
    public async Task AuditClaimAsync(string claimId, string httpRequestType)
    {
        await _queue.EnqueueAsync(
            new AuditMessage(claimId, httpRequestType, AuditEntityType.Claim, DateTime.UtcNow));
    }

    /// <inheritdoc />
    public async Task AuditCoverAsync(string coverId, string httpRequestType)
    {
        await _queue.EnqueueAsync(
            new AuditMessage(coverId, httpRequestType, AuditEntityType.Cover, DateTime.UtcNow));
    }
}
