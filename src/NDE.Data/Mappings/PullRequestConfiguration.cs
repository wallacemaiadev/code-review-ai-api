using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NDE.Domain.Entities.CodeReviews;

namespace NDE.Data.Mappings;

public class PullRequestConfiguration : IEntityTypeConfiguration<PullRequest>
{
  public void Configure(EntityTypeBuilder<PullRequest> builder)
  {
    builder.ToTable("PullRequests");

    builder.HasKey(pr => pr.Id);

    builder.HasIndex(pr => new { pr.RepositoryId, pr.PullRequestId })
           .IsUnique();

    builder.Property(pr => pr.Title)
           .IsRequired()
           .HasColumnType("varchar(300)");

    builder.Property(pr => pr.Description)
           .IsRequired(false)
           .HasColumnType("text");

    builder.Property(pr => pr.RepositoryId)
           .IsRequired();

    builder.Property(pr => pr.PullRequestId)
           .IsRequired();

    builder.Property(pr => pr.TokensConsumed)
           .IsRequired()
           .HasDefaultValue(0);

    builder.Property(pr => pr.CreatedAt)
           .IsRequired();

    builder.Property(pr => pr.UpdatedAt)
           .IsRequired();

    builder.HasOne(pr => pr.Repository)
           .WithMany(r => r.PullRequests)
           .HasForeignKey(pr => pr.RepositoryId)
           .OnDelete(DeleteBehavior.Cascade);

    builder.HasMany(pr => pr.CodeReviews)
           .WithOne(t => t.PullRequest)
           .HasForeignKey(t => t.PullRequestId)
           .OnDelete(DeleteBehavior.Cascade);
  }
}
