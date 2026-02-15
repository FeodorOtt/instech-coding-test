using Claims.Models;
using Microsoft.Extensions.Options;

namespace Claims.Services;

/// <summary>
/// Computes insurance premiums using a progressive discount model.
/// </summary>
/// <remarks>
/// <para>Premium depends on the type of the covered object and the length of the insurance period.</para>
/// <para>Base day rate is configured via <see cref="PremiumSettings.BaseDayRate"/>.</para>
/// <para>Multipliers: Yacht +10%, Passenger ship +20%, Tanker +50%, other types +30%.</para>
/// <para>Progressive discounts by period length:</para>
/// <list type="bullet">
///   <item>First 30 days: no discount (full premium per day).</item>
///   <item>Following 150 days (days 31–180): 5% discount for Yacht, 2% for other types.</item>
///   <item>Remaining days (181+): additional 3% discount for Yacht (8% total), additional 1% for other types (3% total).</item>
/// </list>
/// </remarks>
public class PremiumCalculator : IPremiumCalculator
{
    private readonly decimal _baseDayRate;

    public PremiumCalculator(IOptions<PremiumSettings> options)
    {
        _baseDayRate = options.Value.BaseDayRate;
    }

    private const int FullRateDays = 30;
    private const int MidTierDays = 150;  // days 31–180

    /// <inheritdoc />
    public decimal ComputePremium(DateOnly startDate, DateOnly endDate, CoverType coverType)
    {
        var totalDays = endDate.DayNumber - startDate.DayNumber;
        var premiumPerDay = _baseDayRate * GetMultiplier(coverType);

        var fullRateDays = Math.Min(totalDays, FullRateDays);
        var midTierDays = Math.Min(Math.Max(totalDays - FullRateDays, 0), MidTierDays);
        var lastTierDays = Math.Max(totalDays - FullRateDays - MidTierDays, 0);

        var midTierDiscount = coverType == CoverType.Yacht ? 0.05m : 0.02m;
        var lastTierDiscount = coverType == CoverType.Yacht ? 0.08m : 0.03m;

        return fullRateDays * premiumPerDay
             + midTierDays * premiumPerDay * (1 - midTierDiscount)
             + lastTierDays * premiumPerDay * (1 - lastTierDiscount);
    }

    private static decimal GetMultiplier(CoverType coverType) => coverType switch
    {
        CoverType.Yacht => 1.1m,
        CoverType.PassengerShip => 1.2m,
        CoverType.Tanker => 1.5m,
        _ => 1.3m
    };
}
