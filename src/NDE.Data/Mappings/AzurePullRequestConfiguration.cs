using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NDE.Domain.Entities.CodeReviews;

namespace NDE.Data.Mappings;

public class AzurePullRequestConfiguration : IEntityTypeConfiguration<AzurePullRequest>
{
  public void Configure(EntityTypeBuilder<AzurePullRequest> builder)
  {
    builder.ToTable("PullRequests");
    builder.HasKey(pr => pr.Id);

    builder.Property(pr => pr.RepositoryId).IsRequired();

    builder.Property(pr => pr.CreatedAt)
           .IsRequired()
           .HasColumnName("CreatedAt");

    builder.HasIndex(pr => new { pr.RepositoryId, pr.Id }).IsUnique();

    builder.HasOne(pr => pr.AzureRepository)
           .WithMany(r => r.PullRequests)
           .HasForeignKey(pr => pr.RepositoryId)
           .OnDelete(DeleteBehavior.Cascade);

    builder.HasMany(pr => pr.CodeReviews)
           .WithOne(t => t.AzurePullRequest)
           .HasForeignKey(t => t.PullRequestId)
           .OnDelete(DeleteBehavior.Cascade);
  }
}
