namespace Claims.Models;

/// <summary>
/// Request model for creating a new claim.
/// The <c>Id</c> is assigned server-side and should not be provided by the caller.
/// </summary>
public class CreateClaimRequest
{
    public required string CoverId { get; set; }
    public DateOnly Created { get; set; }
    public required string Name { get; set; }
    public ClaimType Type { get; set; }
    public decimal DamageCost { get; set; }
}
