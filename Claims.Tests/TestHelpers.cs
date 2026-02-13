using Claims.Auditing;
using Claims.Data;
using Claims.Services;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Claims.Tests;

internal static class CoversServiceTestHelper
{
    public static CoversService Create(
        IAuditer? auditer = null,
        IPremiumCalculator? premiumCalculator = null)
    {
        var options = new DbContextOptionsBuilder<ClaimsContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new ClaimsContext(options);

        return new CoversService(
            context,
            auditer ?? Substitute.For<IAuditer>(),
            premiumCalculator ?? new PremiumCalculator());
    }
}

internal static class ClaimsServiceTestHelper
{
    public static ClaimsService Create(
        IAuditer? auditer = null,
        ICoversService? coversService = null)
    {
        var options = new DbContextOptionsBuilder<ClaimsContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        var context = new ClaimsContext(options);

        return new ClaimsService(
            context,
            auditer ?? Substitute.For<IAuditer>(),
            coversService ?? Substitute.For<ICoversService>());
    }
}
