using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NDE.Domain.Entities.CodeReviews;

namespace NDE.Data.Mappings;

public class AzureProjectConfiguration : IEntityTypeConfiguration<AzureProject>
{
  public void Configure(EntityTypeBuilder<AzureProject> builder)
  {
    builder.ToTable("Projects");
    builder.HasKey(p => p.Id);

    builder.Property(p => p.ProjectName).IsRequired().HasColumnType("varchar(200)");
    builder.Property(p => p.ProjectUrl).IsRequired().HasColumnType("varchar(500)");

    builder.HasMany(p => p.Repositories)
          .WithOne(r => r.AzureProject)
          .HasForeignKey(r => r.ProjectId)
          .OnDelete(DeleteBehavior.Cascade);
  }
}
