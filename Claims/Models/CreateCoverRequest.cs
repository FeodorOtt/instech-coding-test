namespace Claims.Models;

/// <summary>
/// Request model for creating a new cover.
/// Premium is computed server-side and should not be provided by the caller.
/// </summary>
public class CreateCoverRequest
{
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public CoverType Type { get; set; }
}
