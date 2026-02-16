using Claims.Auditing;
using NSubstitute;
using Xunit;

namespace Claims.Tests;

public class AuditerTests
{
    private readonly IAuditQueue _queue = Substitute.For<IAuditQueue>();
    private readonly Auditer _auditer;

    public AuditerTests()
    {
        _auditer = new Auditer(_queue);
    }

    [Fact]
    public async Task AuditClaimAsync_EnqueuesClaimAuditMessage()
    {
        await _auditer.AuditClaimAsync("claim-1", "POST");

        await _queue.Received(1).EnqueueAsync(Arg.Is<AuditMessage>(m =>
            m.EntityId == "claim-1" &&
            m.HttpRequestType == "POST" &&
            m.EntityType == AuditEntityType.Claim));
    }

    [Fact]
    public async Task AuditCoverAsync_EnqueuesCoverAuditMessage()
    {
        await _auditer.AuditCoverAsync("cover-1", "DELETE");

        await _queue.Received(1).EnqueueAsync(Arg.Is<AuditMessage>(m =>
            m.EntityId == "cover-1" &&
            m.HttpRequestType == "DELETE" &&
            m.EntityType == AuditEntityType.Cover));
    }
}
