using Claims.Models;
using Claims.Services;
using Xunit;

namespace Claims.Tests;

public class CoverValidationTests
{
    [Fact]
    public async Task CreateCover_StartDateInPast_ThrowsValidationException()
    {
        var service = CoversServiceTestHelper.Create();
        var request = new CreateCoverRequest
        {
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(30),
            Type = CoverType.Yacht
        };

        var ex = await Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(request));

        Assert.Contains(nameof(request.StartDate), ex.Errors.Keys);
    }

    [Fact]
    public async Task CreateCover_PeriodExceedsOneYear_ThrowsValidationException()
    {
        var service = CoversServiceTestHelper.Create();
        var request = new CreateCoverRequest
        {
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1).AddDays(366),
            Type = CoverType.Yacht
        };

        var ex = await Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(request));

        Assert.Contains(nameof(request.EndDate), ex.Errors.Keys);
    }

    [Fact]
    public async Task CreateCover_StartDateInPastAndPeriodExceedsOneYear_ReturnsBothErrors()
    {
        var service = CoversServiceTestHelper.Create();
        var request = new CreateCoverRequest
        {
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1).AddDays(400),
            Type = CoverType.Yacht
        };

        var ex = await Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(request));

        Assert.Equal(2, ex.Errors.Count);
        Assert.Contains(nameof(request.StartDate), ex.Errors.Keys);
        Assert.Contains(nameof(request.EndDate), ex.Errors.Keys);
    }

    [Fact]
    public async Task CreateCover_ExactlyOneYear_DoesNotThrowPeriodError()
    {
        var service = CoversServiceTestHelper.Create();
        var request = new CreateCoverRequest
        {
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1).AddDays(365),
            Type = CoverType.Yacht
        };

        // Should not throw for period — 365 days is exactly the limit
        // This may still fail for other reasons (e.g., DB) but should not throw ValidationException
        var record = await Record.ExceptionAsync(() => service.CreateAsync(request));

        Assert.True(record is not ValidationException);
    }
}
