using Claims.Models;
using Microsoft.EntityFrameworkCore;
using MongoDB.EntityFrameworkCore.Extensions;

namespace Claims.Data;

/// <summary>
/// Entity Framework database context for the Claims MongoDB database.
/// Manages <see cref="Claim"/> and <see cref="Cover"/> collections.
/// </summary>
public class ClaimsContext : DbContext
{
    public DbSet<Claim> Claims { get; init; }
    public DbSet<Cover> Covers { get; init; }

    public ClaimsContext(DbContextOptions<ClaimsContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Claim>().ToCollection("claims");
        modelBuilder.Entity<Cover>().ToCollection("covers");
    }
}
