using Claims.Auditing;
using Claims.Models;
using Claims.Services;
using NSubstitute;
using Xunit;

namespace Claims.Tests;

public class ClaimsServiceTests
{
    private static ICoversService CreateCoversServiceStub(
        string coverId = "cover-1",
        DateOnly? startDate = null,
        DateOnly? endDate = null)
    {
        var start = startDate ?? new DateOnly(2025, 1, 1);
        var end = endDate ?? new DateOnly(2025, 12, 31);

        var coversService = Substitute.For<ICoversService>();
        coversService.GetByIdAsync(coverId).Returns(new CoverResponse
        {
            Id = coverId,
            StartDate = start,
            EndDate = end,
            Type = CoverType.Yacht,
            Premium = 1000m
        });
        return coversService;
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmpty_WhenNoClaimsExist()
    {
        var service = ClaimsServiceTestHelper.Create();

        var result = await service.GetAllAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllClaims()
    {
        var coversService = CreateCoversServiceStub();
        var service = ClaimsServiceTestHelper.Create(coversService: coversService);

        await service.CreateAsync(new CreateClaimRequest
        {
            CoverId = "cover-1",
            Name = "Claim 1",
            Created = new DateOnly(2025, 6, 1),
            DamageCost = 500m
        });
        await service.CreateAsync(new CreateClaimRequest
        {
            CoverId = "cover-1",
            Name = "Claim 2",
            Created = new DateOnly(2025, 7, 1),
            DamageCost = 1000m
        });

        var result = (await service.GetAllAsync()).ToList();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenClaimDoesNotExist()
    {
        var service = ClaimsServiceTestHelper.Create();

        var result = await service.GetByIdAsync("non-existent");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsClaim_WhenExists()
    {
        var coversService = CreateCoversServiceStub();
        var service = ClaimsServiceTestHelper.Create(coversService: coversService);

        var created = await service.CreateAsync(new CreateClaimRequest
        {
            CoverId = "cover-1",
            Name = "Test Claim",
            Created = new DateOnly(2025, 6, 1),
            Type = ClaimType.Collision,
            DamageCost = 500m
        });

        var result = await service.GetByIdAsync(created.Id);

        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
        Assert.Equal("cover-1", result.CoverId);
        Assert.Equal("Test Claim", result.Name);
        Assert.Equal(ClaimType.Collision, result.Type);
        Assert.Equal(500m, result.DamageCost);
        Assert.Equal(new DateOnly(2025, 6, 1), result.Created);
    }

    [Fact]
    public async Task CreateAsync_ReturnsClaimResponse()
    {
        var coversService = CreateCoversServiceStub();
        var service = ClaimsServiceTestHelper.Create(coversService: coversService);

        var result = await service.CreateAsync(new CreateClaimRequest
        {
            CoverId = "cover-1",
            Name = "Test Claim",
            Created = new DateOnly(2025, 6, 1),
            Type = ClaimType.Grounding,
            DamageCost = 750m
        });

        Assert.NotNull(result.Id);
        Assert.Equal("cover-1", result.CoverId);
        Assert.Equal("Test Claim", result.Name);
        Assert.Equal(ClaimType.Grounding, result.Type);
        Assert.Equal(750m, result.DamageCost);
    }

    [Fact]
    public async Task CreateAsync_CallsAuditClaimWithPost()
    {
        var auditer = Substitute.For<IAuditer>();
        var coversService = CreateCoversServiceStub();
        var service = ClaimsServiceTestHelper.Create(auditer: auditer, coversService: coversService);

        var result = await service.CreateAsync(new CreateClaimRequest
        {
            CoverId = "cover-1",
            Name = "Test Claim",
            Created = new DateOnly(2025, 6, 1),
            DamageCost = 500m
        });

        await auditer.Received(1).AuditClaimAsync(result.Id, "POST");
    }

    [Fact]
    public async Task DeleteAsync_RemovesClaim_WhenExists()
    {
        var coversService = CreateCoversServiceStub();
        var service = ClaimsServiceTestHelper.Create(coversService: coversService);

        var created = await service.CreateAsync(new CreateClaimRequest
        {
            CoverId = "cover-1",
            Name = "Test Claim",
            Created = new DateOnly(2025, 6, 1),
            DamageCost = 500m
        });

        await service.DeleteAsync(created.Id);

        var result = await service.GetByIdAsync(created.Id);
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_CallsAuditClaimWithDelete()
    {
        var auditer = Substitute.For<IAuditer>();
        var coversService = CreateCoversServiceStub();
        var service = ClaimsServiceTestHelper.Create(auditer: auditer, coversService: coversService);

        var created = await service.CreateAsync(new CreateClaimRequest
        {
            CoverId = "cover-1",
            Name = "Test Claim",
            Created = new DateOnly(2025, 6, 1),
            DamageCost = 500m
        });

        await service.DeleteAsync(created.Id);

        await auditer.Received(1).AuditClaimAsync(created.Id, "DELETE");
    }

    [Fact]
    public async Task DeleteAsync_DoesNothing_WhenClaimDoesNotExist()
    {
        var auditer = Substitute.For<IAuditer>();
        var service = ClaimsServiceTestHelper.Create(auditer: auditer);

        await service.DeleteAsync("non-existent");

        await auditer.DidNotReceive().AuditClaimAsync(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task CreateAsync_CreatedOnCoverStartDate_Succeeds()
    {
        var coversService = CreateCoversServiceStub(startDate: new DateOnly(2025, 3, 1), endDate: new DateOnly(2025, 12, 31));
        var service = ClaimsServiceTestHelper.Create(coversService: coversService);

        var result = await service.CreateAsync(new CreateClaimRequest
        {
            CoverId = "cover-1",
            Name = "Edge Claim",
            Created = new DateOnly(2025, 3, 1),
            DamageCost = 500m
        });

        Assert.NotNull(result);
    }

    [Fact]
    public async Task CreateAsync_CreatedOnCoverEndDate_Succeeds()
    {
        var coversService = CreateCoversServiceStub(startDate: new DateOnly(2025, 1, 1), endDate: new DateOnly(2025, 6, 30));
        var service = ClaimsServiceTestHelper.Create(coversService: coversService);

        var result = await service.CreateAsync(new CreateClaimRequest
        {
            CoverId = "cover-1",
            Name = "Edge Claim",
            Created = new DateOnly(2025, 6, 30),
            DamageCost = 500m
        });

        Assert.NotNull(result);
    }
}
