using Claims.Models;

namespace Claims.Services;

/// <summary>
/// Service for managing insurance claims.
/// </summary>
public interface IClaimsService
{
    /// <summary>
    /// Retrieves all claims.
    /// </summary>
    Task<IEnumerable<Claim>> GetAllAsync();

    /// <summary>
    /// Retrieves a single claim by its identifier.
    /// </summary>
    /// <param name="id">The unique claim identifier.</param>
    /// <returns>The matching claim, or <c>null</c> if not found.</returns>
    Task<Claim?> GetByIdAsync(string id);

    /// <summary>
    /// Creates a new claim.
    /// </summary>
    /// <param name="claim">The claim to create. The <see cref="Claim.Id"/> will be assigned automatically.</param>
    /// <returns>The created claim with its assigned identifier.</returns>
    Task<Claim> CreateAsync(Claim claim);

    /// <summary>
    /// Deletes a claim by its identifier.
    /// </summary>
    /// <param name="id">The unique claim identifier.</param>
    Task DeleteAsync(string id);
}
