using Claims.Auditing;
using Claims.Data;
using Claims.Models;
using Microsoft.EntityFrameworkCore;

namespace Claims.Services;

/// <summary>
/// Manages CRUD operations for insurance claims, including audit logging.
/// </summary>
public class ClaimsService : IClaimsService
{
    private readonly ClaimsContext _context;
    private readonly IAuditer _auditer;
    private readonly ICoversService _coversService;

    public ClaimsService(ClaimsContext context, IAuditer auditer, ICoversService coversService)
    {
        _context = context;
        _auditer = auditer;
        _coversService = coversService;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Claim>> GetAllAsync()
    {
        return await _context.Claims.ToListAsync();
    }

    /// <inheritdoc />
    public async Task<Claim?> GetByIdAsync(string id)
    {
        return await _context.Claims
            .Where(c => c.Id == id)
            .SingleOrDefaultAsync();
    }

    /// <inheritdoc />
    public async Task<Claim> CreateAsync(Claim claim)
    {
        await ValidateClaimAsync(claim);

        claim.Id = Guid.NewGuid().ToString();
        _context.Claims.Add(claim);
        await _context.SaveChangesAsync();
        await _auditer.AuditClaimAsync(claim.Id, "POST");
        return claim;
    }

    /// <inheritdoc />
    public async Task DeleteAsync(string id)
    {
        var claim = await GetByIdAsync(id);
        if (claim is not null)
        {
            _context.Claims.Remove(claim);
            await _context.SaveChangesAsync();
            await _auditer.AuditClaimAsync(id, "DELETE");
        }
    }

    private async Task ValidateClaimAsync(Claim claim)
    {
        var errors = new Dictionary<string, string>();

        if (claim.DamageCost > 100_000)
            errors.Add(nameof(claim.DamageCost), "Damage cost cannot exceed 100,000.");

        var cover = await _coversService.GetByIdAsync(claim.CoverId);
        if (cover is null)
        {
            errors.Add(nameof(claim.CoverId), "Related cover not found.");
        }
        else if (claim.Created < cover.StartDate || claim.Created > cover.EndDate)
        {
            errors.Add(nameof(claim.Created), "Created date must be within the period of the related cover.");
        }

        if (errors.Count > 0)
            throw new ValidationException(errors);
    }
}
