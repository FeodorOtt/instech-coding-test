namespace Claims.Services;

public class ClaimsSettings
{
    public const string SectionName = "ClaimsSettings";

    public decimal MaxDamageCost { get; set; } = 100_000;
}