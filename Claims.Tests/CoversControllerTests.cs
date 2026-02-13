using Claims.Controllers;
using Claims.Models;
using Claims.Services;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace Claims.Tests;

public class CoversControllerTests
{
    private readonly ICoversService _coversService = Substitute.For<ICoversService>();
    private readonly IPremiumCalculator _premiumCalculator = Substitute.For<IPremiumCalculator>();
    private readonly CoversController _controller;

    public CoversControllerTests()
    {
        _controller = new CoversController(_coversService, _premiumCalculator);
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithCovers()
    {
        var covers = new List<Cover> { new() { Id = "1" } };
        _coversService.GetAllAsync().Returns(covers);

        var result = await _controller.GetAsync();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(covers, okResult.Value);
    }

    [Fact]
    public async Task GetById_ExistingCover_ReturnsOk()
    {
        var cover = new Cover { Id = "1" };
        _coversService.GetByIdAsync("1").Returns(cover);

        var result = await _controller.GetAsync("1");

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(cover, okResult.Value);
    }

    [Fact]
    public async Task GetById_NonExistingCover_ReturnsNotFound()
    {
        _coversService.GetByIdAsync("missing").Returns((Cover?)null);

        var result = await _controller.GetAsync("missing");

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_ReturnsOkWithCreatedCover()
    {
        var request = new CreateCoverRequest { StartDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(1), EndDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(30), Type = CoverType.Yacht };
        var created = new Cover { Id = "new-id", Premium = 50000m };
        _coversService.CreateAsync(request).Returns(created);

        var result = await _controller.CreateAsync(request);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(created, okResult.Value);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent()
    {
        var result = await _controller.DeleteAsync("1");

        Assert.IsType<NoContentResult>(result);
        await _coversService.Received(1).DeleteAsync("1");
    }

    [Fact]
    public void ComputePremium_ReturnsOkWithValue()
    {
        var start = new DateOnly(2025, 6, 1);
        var end = new DateOnly(2025, 7, 1);
        _premiumCalculator.ComputePremium(start, end, CoverType.Yacht).Returns(41250m);

        var result = _controller.ComputePremiumAsync(start, end, CoverType.Yacht);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(41250m, okResult.Value);
    }
}
