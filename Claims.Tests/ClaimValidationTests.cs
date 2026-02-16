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
        coversService.GetByIdAsync("cover-1").Returns(new CoverResponse
        {
            Id = "cover-1",
            StartDate = new DateOnly(2025, 1, 1),
            EndDate = new DateOnly(2025, 12, 31)
        });

        var service = ClaimsServiceTestHelper.Create(coversService: coversService);
        var request = new CreateClaimRequest
        {
            CoverId = "cover-1",
            Name = "Test Claim",
            Created = new DateOnly(2025, 6, 1),
            DamageCost = 100_001m
        };

        var ex = await Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(request));

        Assert.Contains(nameof(request.DamageCost), ex.Errors.Keys);
    }

    [Fact]
    public async Task CreateClaim_DamageCostExactly100000_DoesNotThrowForDamageCost()
    {
        var coversService = Substitute.For<ICoversService>();
        coversService.GetByIdAsync("cover-1").Returns(new CoverResponse
        {
            Id = "cover-1",
            StartDate = new DateOnly(2025, 1, 1),
            EndDate = new DateOnly(2025, 12, 31)
        });

        var service = ClaimsServiceTestHelper.Create(coversService: coversService);
        var request = new CreateClaimRequest
        {
            CoverId = "cover-1",
            Name = "Test Claim",
            Created = new DateOnly(2025, 6, 1),
            DamageCost = 100_000m
        };

        // Should not throw ValidationException — 100,000 is exactly the limit
        var record = await Record.ExceptionAsync(() => service.CreateAsync(request));

        Assert.True(record is not ValidationException);
    }

    [Fact]
    public async Task CreateClaim_CoverNotFound_ThrowsValidationException()
    {
        var coversService = Substitute.For<ICoversService>();
        coversService.GetByIdAsync("missing").Returns((CoverResponse?)null);

        var service = ClaimsServiceTestHelper.Create(coversService: coversService);
        var request = new CreateClaimRequest
        {
            CoverId = "missing",
            Name = "Test Claim",
            Created = new DateOnly(2025, 6, 1),
            DamageCost = 500m
        };

        var ex = await Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(request));

        Assert.Contains(nameof(request.CoverId), ex.Errors.Keys);
    }

    [Fact]
    public async Task CreateClaim_CreatedBeforeCoverStart_ThrowsValidationException()
    {
        var coversService = Substitute.For<ICoversService>();
        coversService.GetByIdAsync("cover-1").Returns(new CoverResponse
        {
            Id = "cover-1",
            StartDate = new DateOnly(2025, 3, 1),
            EndDate = new DateOnly(2025, 12, 31)
        });

        var service = ClaimsServiceTestHelper.Create(coversService: coversService);
        var request = new CreateClaimRequest
        {
            CoverId = "cover-1",
            Name = "Test Claim",
            Created = new DateOnly(2025, 2, 28),
            DamageCost = 500m
        };

        var ex = await Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(request));

        Assert.Contains(nameof(request.Created), ex.Errors.Keys);
    }

    [Fact]
    public async Task CreateClaim_CreatedAfterCoverEnd_ThrowsValidationException()
    {
        var coversService = Substitute.For<ICoversService>();
        coversService.GetByIdAsync("cover-1").Returns(new CoverResponse
        {
            Id = "cover-1",
            StartDate = new DateOnly(2025, 1, 1),
            EndDate = new DateOnly(2025, 6, 30)
        });

        var service = ClaimsServiceTestHelper.Create(coversService: coversService);
        var request = new CreateClaimRequest
        {
            CoverId = "cover-1",
            Name = "Test Claim",
            Created = new DateOnly(2025, 7, 1),
            DamageCost = 500m
        };

        var ex = await Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(request));

        Assert.Contains(nameof(request.Created), ex.Errors.Keys);
    }

    [Fact]
    public async Task CreateClaim_DamageCostExceededAndCoverMissing_ReturnsBothErrors()
    {
        var coversService = Substitute.For<ICoversService>();
        coversService.GetByIdAsync("missing").Returns((CoverResponse?)null);

        var service = ClaimsServiceTestHelper.Create(coversService: coversService);
        var request = new CreateClaimRequest
        {
            CoverId = "missing",
            Name = "Test Claim",
            Created = new DateOnly(2025, 6, 1),
            DamageCost = 200_000m
        };

        var ex = await Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(request));

        Assert.Equal(2, ex.Errors.Count);
        Assert.Contains(nameof(request.DamageCost), ex.Errors.Keys);
        Assert.Contains(nameof(request.CoverId), ex.Errors.Keys);
    }
}
