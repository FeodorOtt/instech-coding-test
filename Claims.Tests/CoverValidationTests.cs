using Claims.Models;
using Claims.Services;
using Xunit;

namespace Claims.Tests;

public class CoverValidationTests
{
    [Fact]
    public void CreateCover_StartDateInPast_ThrowsValidationException()
    {
        var service = CoversServiceTestHelper.Create();
        var request = new CreateCoverRequest
        {
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(30),
            Type = CoverType.Yacht
        };

        var ex = Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(request)).Result;

        Assert.Contains(nameof(request.StartDate), ex.Errors.Keys);
    }

    [Fact]
    public void CreateCover_PeriodExceedsOneYear_ThrowsValidationException()
    {
        var service = CoversServiceTestHelper.Create();
        var request = new CreateCoverRequest
        {
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1).AddDays(366),
            Type = CoverType.Yacht
        };

        var ex = Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(request)).Result;

        Assert.Contains(nameof(request.EndDate), ex.Errors.Keys);
    }

    [Fact]
    public void CreateCover_StartDateInPastAndPeriodExceedsOneYear_ReturnsBothErrors()
    {
        var service = CoversServiceTestHelper.Create();
        var request = new CreateCoverRequest
        {
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1),
            EndDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1).AddDays(400),
            Type = CoverType.Yacht
        };

        var ex = Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(request)).Result;

        Assert.Equal(2, ex.Errors.Count);
        Assert.Contains(nameof(request.StartDate), ex.Errors.Keys);
        Assert.Contains(nameof(request.EndDate), ex.Errors.Keys);
    }

    [Fact]
    public void CreateCover_ExactlyOneYear_DoesNotThrowPeriodError()
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
        var record = Record.ExceptionAsync(() => service.CreateAsync(request)).Result;

        Assert.True(record is not ValidationException);
    }
}
