using Claims.Services;
using Xunit;

namespace Claims.Tests;

public class PremiumCalculatorTests
{
    private readonly PremiumCalculator _calculator = new();

    [Theory]
    [InlineData(CoverType.Yacht, 1375)]        // 1250 * 1.1
    [InlineData(CoverType.PassengerShip, 1500)] // 1250 * 1.2
    [InlineData(CoverType.Tanker, 1875)]        // 1250 * 1.5
    [InlineData(CoverType.ContainerShip, 1625)] // 1250 * 1.3
    [InlineData(CoverType.BulkCarrier, 1625)]   // 1250 * 1.3
    public void ComputePremium_SingleDay_ReturnsCorrectBaseRate(CoverType coverType, decimal expectedDayRate)
    {
        var start = new DateOnly(2025, 1, 1);
        var end = start.AddDays(1);

        var premium = _calculator.ComputePremium(start, end, coverType);

        Assert.Equal(expectedDayRate, premium);
    }

    [Fact]
    public void ComputePremium_First30Days_NoDiscount()
    {
        var start = new DateOnly(2025, 1, 1);
        var end = start.AddDays(30);

        var premium = _calculator.ComputePremium(start, end, CoverType.Yacht);

        // 30 days * 1250 * 1.1 = 41250
        Assert.Equal(30 * 1375m, premium);
    }

    [Fact]
    public void ComputePremium_Days31To180_Yacht_5PercentDiscount()
    {
        var start = new DateOnly(2025, 1, 1);
        var end = start.AddDays(60);
        var baseDayRate = 1250m * 1.1m; // 1375

        var premium = _calculator.ComputePremium(start, end, CoverType.Yacht);

        // 30 full-rate days + 30 discounted days
        var expected = 30 * baseDayRate + 30 * baseDayRate * 0.95m;
        Assert.Equal(expected, premium);
    }

    [Fact]
    public void ComputePremium_Days31To180_NonYacht_2PercentDiscount()
    {
        var start = new DateOnly(2025, 1, 1);
        var end = start.AddDays(60);
        var baseDayRate = 1250m * 1.3m; // 1625

        var premium = _calculator.ComputePremium(start, end, CoverType.ContainerShip);

        var expected = 30 * baseDayRate + 30 * baseDayRate * 0.98m;
        Assert.Equal(expected, premium);
    }

    [Fact]
    public void ComputePremium_Days181Plus_Yacht_8PercentTotalDiscount()
    {
        var start = new DateOnly(2025, 1, 1);
        var end = start.AddDays(200);
        var baseDayRate = 1250m * 1.1m;

        var premium = _calculator.ComputePremium(start, end, CoverType.Yacht);

        var expected = 30 * baseDayRate
                     + 150 * baseDayRate * 0.95m
                     + 20 * baseDayRate * 0.92m;
        Assert.Equal(expected, premium);
    }

    [Fact]
    public void ComputePremium_Days181Plus_NonYacht_3PercentTotalDiscount()
    {
        var start = new DateOnly(2025, 1, 1);
        var end = start.AddDays(200);
        var baseDayRate = 1250m * 1.5m; // Tanker

        var premium = _calculator.ComputePremium(start, end, CoverType.Tanker);

        var expected = 30 * baseDayRate
                     + 150 * baseDayRate * 0.98m
                     + 20 * baseDayRate * 0.97m;
        Assert.Equal(expected, premium);
    }

    [Fact]
    public void ComputePremium_FullYear_365Days()
    {
        var start = new DateOnly(2025, 1, 1);
        var end = start.AddDays(365);
        var baseDayRate = 1250m * 1.1m;

        var premium = _calculator.ComputePremium(start, end, CoverType.Yacht);

        var expected = 30 * baseDayRate
                     + 150 * baseDayRate * 0.95m
                     + 185 * baseDayRate * 0.92m;
        Assert.Equal(expected, premium);
    }

    [Fact]
    public void ComputePremium_ZeroDays_ReturnsZero()
    {
        var date = new DateOnly(2025, 1, 1);

        var premium = _calculator.ComputePremium(date, date, CoverType.Yacht);

        Assert.Equal(0m, premium);
    }
}
