using Claims.Models;

namespace Claims.Services;

/// <summary>
/// Service for managing insurance covers.
/// </summary>
public interface ICoversService
{
    /// <summary>
    /// Retrieves all covers.
    /// </summary>
    Task<IEnumerable<CoverResponse>> GetAllAsync();

    /// <summary>
    /// Retrieves a single cover by its identifier.
    /// </summary>
    /// <param name="id">The unique cover identifier.</param>
    /// <returns>The matching cover, or <c>null</c> if not found.</returns>
    Task<CoverResponse?> GetByIdAsync(string id);

    /// <summary>
    /// Creates a new cover, computing its premium automatically.
    /// </summary>
    /// <param name="request">The cover creation request containing start date, end date, and type.</param>
    /// <returns>The created cover with computed premium.</returns>
    Task<CoverResponse> CreateAsync(CreateCoverRequest request);

    /// <summary>
    /// Deletes a cover by its identifier.
    /// </summary>
    /// <param name="id">The unique cover identifier.</param>
    Task DeleteAsync(string id);
}
