using Claims.Models;
using Claims.Services;
using NSubstitute;
using Xunit;

namespace Claims.Tests;

public class ClaimValidationTests
{
    [Fact]
    public async Task CreateClaim_DamageCostExceeds100000_ThrowsValidationException()
    {
        var coversService = Substitute.For<ICoversService>();
        coversService.GetByIdAsync("cover-1").Returns(new Cover
        {
            Id = "cover-1",
            StartDate = new DateOnly(2025, 1, 1),
            EndDate = new DateOnly(2025, 12, 31)
        });

        var service = ClaimsServiceTestHelper.Create(coversService: coversService);
        var claim = new Claim
        {
            CoverId = "cover-1",
            Created = new DateOnly(2025, 6, 1),
            DamageCost = 100_001m
        };

        var ex = await Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(claim));

        Assert.Contains(nameof(Claim.DamageCost), ex.Errors.Keys);
    }

    [Fact]
    public async Task CreateClaim_DamageCostExactly100000_DoesNotThrowForDamageCost()
    {
        var coversService = Substitute.For<ICoversService>();
        coversService.GetByIdAsync("cover-1").Returns(new Cover
        {
            Id = "cover-1",
            StartDate = new DateOnly(2025, 1, 1),
            EndDate = new DateOnly(2025, 12, 31)
        });

        var service = ClaimsServiceTestHelper.Create(coversService: coversService);
        var claim = new Claim
        {
            CoverId = "cover-1",
            Created = new DateOnly(2025, 6, 1),
            DamageCost = 100_000m
        };

        // Should not throw ValidationException — 100,000 is exactly the limit
        var record = await Record.ExceptionAsync(() => service.CreateAsync(claim));

        Assert.True(record is not ValidationException);
    }

    [Fact]
    public async Task CreateClaim_CoverNotFound_ThrowsValidationException()
    {
        var coversService = Substitute.For<ICoversService>();
        coversService.GetByIdAsync("missing").Returns((Cover?)null);

        var service = ClaimsServiceTestHelper.Create(coversService: coversService);
        var claim = new Claim
        {
            CoverId = "missing",
            Created = new DateOnly(2025, 6, 1),
            DamageCost = 500m
        };

        var ex = await Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(claim));

        Assert.Contains(nameof(Claim.CoverId), ex.Errors.Keys);
    }

    [Fact]
    public async Task CreateClaim_CreatedBeforeCoverStart_ThrowsValidationException()
    {
        var coversService = Substitute.For<ICoversService>();
        coversService.GetByIdAsync("cover-1").Returns(new Cover
        {
            Id = "cover-1",
            StartDate = new DateOnly(2025, 3, 1),
            EndDate = new DateOnly(2025, 12, 31)
        });

        var service = ClaimsServiceTestHelper.Create(coversService: coversService);
        var claim = new Claim
        {
            CoverId = "cover-1",
            Created = new DateOnly(2025, 2, 28),
            DamageCost = 500m
        };

        var ex = await Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(claim));

        Assert.Contains(nameof(Claim.Created), ex.Errors.Keys);
    }

    [Fact]
    public async Task CreateClaim_CreatedAfterCoverEnd_ThrowsValidationException()
    {
        var coversService = Substitute.For<ICoversService>();
        coversService.GetByIdAsync("cover-1").Returns(new Cover
        {
            Id = "cover-1",
            StartDate = new DateOnly(2025, 1, 1),
            EndDate = new DateOnly(2025, 6, 30)
        });

        var service = ClaimsServiceTestHelper.Create(coversService: coversService);
        var claim = new Claim
        {
            CoverId = "cover-1",
            Created = new DateOnly(2025, 7, 1),
            DamageCost = 500m
        };

        var ex = await Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(claim));

        Assert.Contains(nameof(Claim.Created), ex.Errors.Keys);
    }

    [Fact]
    public async Task CreateClaim_DamageCostExceededAndCoverMissing_ReturnsBothErrors()
    {
        var coversService = Substitute.For<ICoversService>();
        coversService.GetByIdAsync("missing").Returns((Cover?)null);

        var service = ClaimsServiceTestHelper.Create(coversService: coversService);
        var claim = new Claim
        {
            CoverId = "missing",
            Created = new DateOnly(2025, 6, 1),
            DamageCost = 200_000m
        };

        var ex = await Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(claim));

        Assert.Equal(2, ex.Errors.Count);
        Assert.Contains(nameof(Claim.DamageCost), ex.Errors.Keys);
        Assert.Contains(nameof(Claim.CoverId), ex.Errors.Keys);
    }
}
