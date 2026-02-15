namespace Claims.Models;

/// <summary>
/// Response model representing an insurance claim.
/// </summary>
public class ClaimResponse
{
    public required string Id { get; set; }
    public required string CoverId { get; set; }
    public DateOnly Created { get; set; }
    public required string Name { get; set; }
    public ClaimType Type { get; set; }
    public decimal DamageCost { get; set; }
}
