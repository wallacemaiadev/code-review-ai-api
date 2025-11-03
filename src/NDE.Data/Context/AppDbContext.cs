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

  public DbSet<AzureProject> AzureProjects { get; set; } = default!;
  public DbSet<AzureRepository> AzureRepositories { get; set; } = default!;
  public DbSet<AzurePullRequest> PullRequests { get; set; } = default!;
  public DbSet<CodeReview> CodeReviews { get; set; } = default!;

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    foreach (var property in modelBuilder.Model.GetEntityTypes()
        .SelectMany(e => e.GetProperties()
            .Where(p => p.ClrType == typeof(string))))
      property.SetColumnType("varchar(100)");

    modelBuilder.ApplyConfiguration(new AzureProjectConfiguration());
    modelBuilder.ApplyConfiguration(new AzureRepositoryConfiguration());
    modelBuilder.ApplyConfiguration(new AzurePullRequestConfiguration());
    modelBuilder.ApplyConfiguration(new CodeReviewConfiguration());

    base.OnModelCreating(modelBuilder);
  }
}
