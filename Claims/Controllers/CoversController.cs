using Claims.Models;
using Claims.Services;
using Microsoft.AspNetCore.Mvc;

namespace Claims.Controllers;

/// <summary>
/// REST API controller for managing insurance covers.
/// </summary>
[ApiController]
[Route("[controller]")]
public class CoversController : ControllerBase
{
    private readonly ICoversService _coversService;
    private readonly IPremiumCalculator _premiumCalculator;

    public CoversController(ICoversService coversService, IPremiumCalculator premiumCalculator)
    {
        _coversService = coversService;
        _premiumCalculator = premiumCalculator;
    }

    /// <summary>
    /// Computes an insurance premium for a given period and cover type without creating a cover.
    /// </summary>
    /// <param name="startDate">Start date of the insurance period.</param>
    /// <param name="endDate">End date of the insurance period.</param>
    /// <param name="coverType">The type of cover.</param>
    [HttpPost("compute")]
    public async Task<ActionResult<decimal>> ComputePremiumAsync(DateOnly startDate, DateOnly endDate, CoverType coverType)
    {
        return Ok(await Task.FromResult(_premiumCalculator.ComputePremium(startDate, endDate, coverType)));
    }

    /// <summary>
    /// Retrieves all covers.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Cover>>> GetAsync()
    {
        var covers = await _coversService.GetAllAsync();
        return Ok(covers);
    }

    /// <summary>
    /// Retrieves a single cover by its identifier.
    /// </summary>
    /// <param name="id">The unique cover identifier.</param>
    [HttpGet("{id}")]
    public async Task<ActionResult<Cover>> GetAsync(string id)
    {
        var cover = await _coversService.GetByIdAsync(id);
        if (cover is null)
            return NotFound();

        return Ok(cover);
    }

    /// <summary>
    /// Creates a new cover. The premium is computed automatically.
    /// </summary>
    /// <param name="request">The cover creation request (start date, end date, and type).</param>
    [HttpPost]
    public async Task<ActionResult<Cover>> CreateAsync(CreateCoverRequest request)
    {
        var created = await _coversService.CreateAsync(request);
        return Ok(created);
    }

    /// <summary>
    /// Deletes a cover by its identifier.
    /// </summary>
    /// <param name="id">The unique cover identifier.</param>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(string id)
    {
        await _coversService.DeleteAsync(id);
        return NoContent();
    }
}
