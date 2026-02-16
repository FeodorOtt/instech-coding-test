namespace Claims.Auditing;

/// <summary>
/// Represents an audit event to be processed asynchronously.
/// </summary>
/// <param name="EntityId">The identifier of the audited entity (claim or cover).</param>
/// <param name="HttpRequestType">The HTTP method that triggered the operation.</param>
/// <param name="EntityType">Discriminator indicating whether this audit is for a claim or cover.</param>
/// <param name="Timestamp">The UTC time the operation occurred.</param>
public record AuditMessage(string EntityId, string HttpRequestType, AuditEntityType EntityType, DateTime Timestamp);

/// <summary>
/// Identifies the type of entity being audited.
/// </summary>
public enum AuditEntityType
{
    Claim,
    Cover
}
