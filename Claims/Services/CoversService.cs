using Claims.Auditing;
using Claims.Data;
using Claims.Models;
using Microsoft.EntityFrameworkCore;

namespace Claims.Services;

/// <summary>
/// Manages CRUD operations for insurance covers, including premium computation and audit logging.
/// </summary>
public class CoversService : ICoversService
{
    private readonly ClaimsContext _context;
    private readonly IAuditer _auditer;
    private readonly IPremiumCalculator _premiumCalculator;

    public CoversService(ClaimsContext context, IAuditer auditer, IPremiumCalculator premiumCalculator)
    {
        _context = context;
        _auditer = auditer;
        _premiumCalculator = premiumCalculator;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<CoverResponse>> GetAllAsync()
    {
        var covers = await _context.Covers.ToListAsync();
        return covers.Select(ToResponse);
    }

    /// <inheritdoc />
    public async Task<CoverResponse?> GetByIdAsync(string id)
    {
        var cover = await _context.Covers
            .Where(c => c.Id == id)
            .SingleOrDefaultAsync();

        return cover is null ? null : ToResponse(cover);
    }

    /// <inheritdoc />
    public async Task<CoverResponse> CreateAsync(CreateCoverRequest request)
    {
        ValidateCreateRequest(request);

        var cover = new Cover
        {
            Id = Guid.NewGuid().ToString(),
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Type = request.Type,
            Premium = _premiumCalculator.ComputePremium(request.StartDate, request.EndDate, request.Type)
        };

        _context.Covers.Add(cover);
        await _context.SaveChangesAsync();
        await _auditer.AuditCoverAsync(cover.Id, "POST");
        return ToResponse(cover);
    }

    private static void ValidateCreateRequest(CreateCoverRequest request)
    {
        var errors = new Dictionary<string, string>();

        if (request.StartDate < DateOnly.FromDateTime(DateTime.UtcNow))
            errors.Add(nameof(request.StartDate), "Start date cannot be in the past.");

        if (request.EndDate <= request.StartDate)
            errors.Add(nameof(request.EndDate), "End date must be after start date.");

        if (request.EndDate.DayNumber - request.StartDate.DayNumber > 365)
            errors.Add(nameof(request.EndDate), "Total insurance period cannot exceed 1 year.");

        if (errors.Count > 0)
            throw new ValidationException(errors);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(string id)
    {
        var cover = await _context.Covers
            .Where(c => c.Id == id)
            .SingleOrDefaultAsync();

        if (cover is not null)
        {
            _context.Covers.Remove(cover);
            await _context.SaveChangesAsync();
            await _auditer.AuditCoverAsync(id, "DELETE");
        }
    }

    private static CoverResponse ToResponse(Cover cover) => new()
    {
        Id = cover.Id,
        StartDate = cover.StartDate,
        EndDate = cover.EndDate,
        Type = cover.Type,
        Premium = cover.Premium
    };
}
