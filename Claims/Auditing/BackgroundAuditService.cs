using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Claims.Auditing;

/// <summary>
/// Long-running background service that reads audit messages from an
/// <see cref="IAuditQueue"/> and persists them to the database.
/// This decouples audit persistence from HTTP request processing.
/// </summary>
public class BackgroundAuditService : BackgroundService
{
    private readonly IAuditQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BackgroundAuditService> _logger;

    public BackgroundAuditService(IAuditQueue queue, IServiceScopeFactory scopeFactory, ILogger<BackgroundAuditService> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var message in _queue.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AuditContext>();

                switch (message.EntityType)
                {
                    case AuditEntityType.Claim:
                        context.ClaimAudits.Add(new ClaimAudit
                        {
                            ClaimId = message.EntityId,
                            HttpRequestType = message.HttpRequestType,
                            Created = message.Timestamp
                        });
                        break;

                    case AuditEntityType.Cover:
                        context.CoverAudits.Add(new CoverAudit
                        {
                            CoverId = message.EntityId,
                            HttpRequestType = message.HttpRequestType,
                            Created = message.Timestamp
                        });
                        break;
                }

                await context.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to persist audit message for {EntityType} {EntityId}",
                    message.EntityType, message.EntityId);
            }
        }
    }
}
