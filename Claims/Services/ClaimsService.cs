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
    public async Task<IEnumerable<ClaimResponse>> GetAllAsync()
    {
        var claims = await _context.Claims.ToListAsync();
        return claims.Select(ToResponse);
    }

    /// <inheritdoc />
    public async Task<ClaimResponse?> GetByIdAsync(string id)
    {
        var claim = await _context.Claims
            .Where(c => c.Id == id)
            .SingleOrDefaultAsync();

        return claim is null ? null : ToResponse(claim);
    }

    /// <inheritdoc />
    public async Task<ClaimResponse> CreateAsync(CreateClaimRequest request)
    {
        await ValidateRequestAsync(request);

        var claim = new Claim
        {
            Id = Guid.NewGuid().ToString(),
            CoverId = request.CoverId,
            Created = request.Created,
            Name = request.Name,
            Type = request.Type,
            DamageCost = request.DamageCost
        };

        _context.Claims.Add(claim);
        await _context.SaveChangesAsync();
        await _auditer.AuditClaimAsync(claim.Id, "POST");
        return ToResponse(claim);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(string id)
    {
        var claim = await _context.Claims
            .Where(c => c.Id == id)
            .SingleOrDefaultAsync();

        if (claim is not null)
        {
            _context.Claims.Remove(claim);
            await _context.SaveChangesAsync();
            await _auditer.AuditClaimAsync(id, "DELETE");
        }
    }

    private async Task ValidateRequestAsync(CreateClaimRequest request)
    {
        var errors = new Dictionary<string, string>();

        if (request.DamageCost > 100_000)
            errors.Add(nameof(request.DamageCost), "Damage cost cannot exceed 100,000.");

        var cover = await _coversService.GetByIdAsync(request.CoverId);
        if (cover is null)
        {
            errors.Add(nameof(request.CoverId), "Related cover not found.");
        }
        else if (request.Created < cover.StartDate || request.Created > cover.EndDate)
        {
            errors.Add(nameof(request.Created), "Created date must be within the period of the related cover.");
        }

        if (errors.Count > 0)
            throw new ValidationException(errors);
    }

    private static ClaimResponse ToResponse(Claim claim) => new()
    {
        Id = claim.Id,
        CoverId = claim.CoverId,
        Created = claim.Created,
        Name = claim.Name,
        Type = claim.Type,
        DamageCost = claim.DamageCost
    };
}
