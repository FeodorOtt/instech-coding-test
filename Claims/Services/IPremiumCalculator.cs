namespace Claims.Services;

/// <summary>
/// Computes insurance premiums for covers.
/// </summary>
public interface IPremiumCalculator
{
    /// <summary>
    /// Computes the total premium for a cover based on type and insurance period.
    /// </summary>
    /// <param name="startDate">The start date of the insurance period.</param>
    /// <param name="endDate">The end date of the insurance period.</param>
    /// <param name="coverType">The type of cover, which affects the premium multiplier.</param>
    /// <returns>The computed premium amount.</returns>
    decimal ComputePremium(DateOnly startDate, DateOnly endDate, CoverType coverType);
}
