using Claims.Controllers;
using Claims.Models;
using Claims.Services;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace Claims.Tests;

public class ClaimsControllerUnitTests
{
    private readonly IClaimsService _claimsService = Substitute.For<IClaimsService>();
    private readonly ClaimsController _controller;

    public ClaimsControllerUnitTests()
    {
        _controller = new ClaimsController(_claimsService);
    }

    [Fact]
    public async Task GetAll_ReturnsOkWithClaims()
    {
        var claims = new List<ClaimResponse> { new() { Id = "1", Name = "Test" } };
        _claimsService.GetAllAsync().Returns(claims);

        var result = await _controller.GetAsync();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(claims, okResult.Value);
    }

    [Fact]
    public async Task GetById_ExistingClaim_ReturnsOk()
    {
        var claim = new ClaimResponse { Id = "1", Name = "Test" };
        _claimsService.GetByIdAsync("1").Returns(claim);

        var result = await _controller.GetAsync("1");

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(claim, okResult.Value);
    }

    [Fact]
    public async Task GetById_NonExistingClaim_ReturnsNotFound()
    {
        _claimsService.GetByIdAsync("missing").Returns((ClaimResponse?)null);

        var result = await _controller.GetAsync("missing");

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_ReturnsOkWithCreatedClaim()
    {
        var request = new CreateClaimRequest { Name = "Test", DamageCost = 500, CoverId = "c1", Created = new DateOnly(2025, 6, 1) };
        var created = new ClaimResponse { Id = "new-id", Name = "Test", DamageCost = 500 };
        _claimsService.CreateAsync(request).Returns(created);

        var result = await _controller.CreateAsync(request);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(created, okResult.Value);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent()
    {
        var result = await _controller.DeleteAsync("1");

        Assert.IsType<NoContentResult>(result);
        await _claimsService.Received(1).DeleteAsync("1");
    }
}
