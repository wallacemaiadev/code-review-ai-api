using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NDE.Domain.Entities.CodeReviews;

namespace NDE.Data.Mappings;

public class AzureRepositoryConfiguration : IEntityTypeConfiguration<AzureRepository>
{
  public void Configure(EntityTypeBuilder<AzureRepository> builder)
  {
    builder.ToTable("Repositories");
    builder.HasKey(r => r.Id);

    builder.Property(r => r.ProjectId).IsRequired();
    builder.Property(r => r.RepositoryName).IsRequired().HasColumnType("varchar(200)");
    builder.Property(r => r.RepositoryUrl).IsRequired().HasColumnType("varchar(500)");

    builder.HasOne(r => r.AzureProject)
       .WithMany(p => p.Repositories)
       .HasForeignKey(r => r.ProjectId)
       .OnDelete(DeleteBehavior.Cascade);

    builder.HasMany(r => r.PullRequests)
      .WithOne(pr => pr.AzureRepository)
      .HasForeignKey(pr => pr.RepositoryId)
      .OnDelete(DeleteBehavior.Cascade);
  }
}
