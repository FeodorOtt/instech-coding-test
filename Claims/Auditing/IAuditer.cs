namespace Claims.Auditing;

/// <summary>
/// Provides asynchronous auditing capabilities for claim and cover operations.
/// </summary>
public interface IAuditer
{
    /// <summary>
    /// Records an audit entry for a claim operation.
    /// </summary>
    /// <param name="claimId">The identifier of the claim being audited.</param>
    /// <param name="httpRequestType">The HTTP method that triggered the operation (e.g., POST, DELETE).</param>
    Task AuditClaimAsync(string claimId, string httpRequestType);

    /// <summary>
    /// Records an audit entry for a cover operation.
    /// </summary>
    /// <param name="coverId">The identifier of the cover being audited.</param>
    /// <param name="httpRequestType">The HTTP method that triggered the operation (e.g., POST, DELETE).</param>
    Task AuditCoverAsync(string coverId, string httpRequestType);
}
