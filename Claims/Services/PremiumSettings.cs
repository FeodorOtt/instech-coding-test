namespace Claims.Services;

public class PremiumSettings
{
    public const string SectionName = "PremiumSettings";

    public decimal BaseDayRate { get; set; } = 1250m;
}
