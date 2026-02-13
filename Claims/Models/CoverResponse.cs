namespace Claims.Models;

/// <summary>
/// Response model representing an insurance cover.
/// </summary>
public class CoverResponse
{
    public string Id { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public CoverType Type { get; set; }
    public decimal Premium { get; set; }
}
