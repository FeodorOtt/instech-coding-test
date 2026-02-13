using Claims.Models;
using Claims.Services;
using Microsoft.AspNetCore.Mvc;

namespace Claims.Controllers;

/// <summary>
/// REST API controller for managing insurance claims.
/// </summary>
[ApiController]
[Route("[controller]")]
public class ClaimsController : ControllerBase
{
    private readonly IClaimsService _claimsService;

    public ClaimsController(IClaimsService claimsService)
    {
        _claimsService = claimsService;
    }

    /// <summary>
    /// Retrieves all claims.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClaimResponse>>> GetAsync()
    {
        var claims = await _claimsService.GetAllAsync();
        return Ok(claims);
    }

    /// <summary>
    /// Retrieves a single claim by its identifier.
    /// </summary>
    /// <param name="id">The unique claim identifier.</param>
    [HttpGet("{id}")]
    public async Task<ActionResult<ClaimResponse>> GetAsync(string id)
    {
        var claim = await _claimsService.GetByIdAsync(id);
        if (claim is null)
            return NotFound();

        return Ok(claim);
    }

    /// <summary>
    /// Creates a new claim.
    /// </summary>
    /// <param name="request">The claim creation request.</param>
    [HttpPost]
    public async Task<ActionResult<ClaimResponse>> CreateAsync(CreateClaimRequest request)
    {
        var created = await _claimsService.CreateAsync(request);
        return Ok(created);
    }

    /// <summary>
    /// Deletes a claim by its identifier.
    /// </summary>
    /// <param name="id">The unique claim identifier.</param>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(string id)
    {
        await _claimsService.DeleteAsync(id);
        return NoContent();
    }
}
