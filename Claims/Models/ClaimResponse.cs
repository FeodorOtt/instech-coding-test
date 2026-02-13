namespace Claims.Models;

/// <summary>
/// Response model representing an insurance claim.
/// </summary>
public class ClaimResponse
{
    public string Id { get; set; }
    public string CoverId { get; set; }
    public DateOnly Created { get; set; }
    public string Name { get; set; }
    public ClaimType Type { get; set; }
    public decimal DamageCost { get; set; }
}
