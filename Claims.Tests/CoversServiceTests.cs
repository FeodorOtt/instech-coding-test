using Claims.Auditing;
using Claims.Models;
using Claims.Services;
using NSubstitute;
using Xunit;

namespace Claims.Tests;

public class CoversServiceTests
{
    [Fact]
    public async Task GetAllAsync_ReturnsEmpty_WhenNoCoversExist()
    {
        var service = CoversServiceTestHelper.Create();

        var result = await service.GetAllAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllCovers()
    {
        var service = CoversServiceTestHelper.Create();
        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);

        await service.CreateAsync(new CreateCoverRequest
        {
            StartDate = tomorrow,
            EndDate = tomorrow.AddDays(30),
            Type = CoverType.Yacht
        });
        await service.CreateAsync(new CreateCoverRequest
        {
            StartDate = tomorrow,
            EndDate = tomorrow.AddDays(60),
            Type = CoverType.Tanker
        });

        var result = (await service.GetAllAsync()).ToList();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenCoverDoesNotExist()
    {
        var service = CoversServiceTestHelper.Create();

        var result = await service.GetByIdAsync("non-existent");

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsCover_WhenExists()
    {
        var service = CoversServiceTestHelper.Create();
        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);
        var created = await service.CreateAsync(new CreateCoverRequest
        {
            StartDate = tomorrow,
            EndDate = tomorrow.AddDays(30),
            Type = CoverType.ContainerShip
        });

        var result = await service.GetByIdAsync(created.Id);

        Assert.NotNull(result);
        Assert.Equal(created.Id, result.Id);
        Assert.Equal(tomorrow, result.StartDate);
        Assert.Equal(tomorrow.AddDays(30), result.EndDate);
        Assert.Equal(CoverType.ContainerShip, result.Type);
    }

    [Fact]
    public async Task CreateAsync_ReturnsResponseWithComputedPremium()
    {
        var service = CoversServiceTestHelper.Create();
        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);

        var result = await service.CreateAsync(new CreateCoverRequest
        {
            StartDate = tomorrow,
            EndDate = tomorrow.AddDays(30),
            Type = CoverType.Yacht
        });

        Assert.NotNull(result.Id);
        Assert.Equal(tomorrow, result.StartDate);
        Assert.Equal(tomorrow.AddDays(30), result.EndDate);
        Assert.Equal(CoverType.Yacht, result.Type);
        Assert.True(result.Premium > 0);
    }

    [Fact]
    public async Task CreateAsync_CallsAuditCoverWithPost()
    {
        var auditer = Substitute.For<IAuditer>();
        var service = CoversServiceTestHelper.Create(auditer: auditer);
        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);

        var result = await service.CreateAsync(new CreateCoverRequest
        {
            StartDate = tomorrow,
            EndDate = tomorrow.AddDays(30),
            Type = CoverType.Yacht
        });

        await auditer.Received(1).AuditCoverAsync(result.Id, "POST");
    }

    [Fact]
    public async Task CreateAsync_EndDateEqualsStartDate_ThrowsValidationException()
    {
        var service = CoversServiceTestHelper.Create();
        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);

        var request = new CreateCoverRequest
        {
            StartDate = tomorrow,
            EndDate = tomorrow,
            Type = CoverType.Yacht
        };

        var ex = await Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(request));

        Assert.Contains(nameof(request.EndDate), ex.Errors.Keys);
    }

    [Fact]
    public async Task DeleteAsync_RemovesCover_WhenExists()
    {
        var service = CoversServiceTestHelper.Create();
        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);
        var created = await service.CreateAsync(new CreateCoverRequest
        {
            StartDate = tomorrow,
            EndDate = tomorrow.AddDays(30),
            Type = CoverType.Yacht
        });

        await service.DeleteAsync(created.Id);

        var result = await service.GetByIdAsync(created.Id);
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_CallsAuditCoverWithDelete()
    {
        var auditer = Substitute.For<IAuditer>();
        var service = CoversServiceTestHelper.Create(auditer: auditer);
        var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1);
        var created = await service.CreateAsync(new CreateCoverRequest
        {
            StartDate = tomorrow,
            EndDate = tomorrow.AddDays(30),
            Type = CoverType.Yacht
        });

        await service.DeleteAsync(created.Id);

        await auditer.Received(1).AuditCoverAsync(created.Id, "DELETE");
    }

    [Fact]
    public async Task DeleteAsync_DoesNothing_WhenCoverDoesNotExist()
    {
        var auditer = Substitute.For<IAuditer>();
        var service = CoversServiceTestHelper.Create(auditer: auditer);

        await service.DeleteAsync("non-existent");

        await auditer.DidNotReceive().AuditCoverAsync(Arg.Any<string>(), Arg.Any<string>());
    }
}
