using Microsoft.EntityFrameworkCore;

using NDE.Data.Mappings;
using NDE.Domain.Entities.CodeReviews;

namespace NDE.Data.Context;

public class AppDbContext : DbContext
{
  public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
  {
    ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    ChangeTracker.AutoDetectChangesEnabled = false;
  }

  public DbSet<Project> Projects { get; set; } = default!;
  public DbSet<Repository> Repositories { get; set; } = default!;
  public DbSet<PullRequest> PullRequests { get; set; } = default!;
  public DbSet<CodeReview> CodeReviews { get; set; } = default!;

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    foreach (var property in modelBuilder.Model.GetEntityTypes()
        .SelectMany(e => e.GetProperties()
            .Where(p => p.ClrType == typeof(string))))
      property.SetColumnType("varchar(100)");

    modelBuilder.ApplyConfiguration(new ProjectConfiguration());
    modelBuilder.ApplyConfiguration(new RepositoryConfiguration());
    modelBuilder.ApplyConfiguration(new PullRequestConfiguration());
    modelBuilder.ApplyConfiguration(new CodeReviewConfiguration());
    modelBuilder.ApplyConfiguration(new CodeModificationConfiguration());

    base.OnModelCreating(modelBuilder);
  }
}
