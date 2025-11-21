using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NDE.Domain.Entities.CodeReviews;

namespace NDE.Data.Mappings;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
  public void Configure(EntityTypeBuilder<Project> builder)
  {
    builder.ToTable("Projects");
    builder.HasKey(p => p.Id);

    builder.Property(p => p.Name).IsRequired().HasColumnType("varchar(200)");
    builder.Property(p => p.Description).IsRequired(false).HasColumnType("text");
    builder.Property(p => p.Url).IsRequired().HasColumnType("varchar(500)");
    builder.Property(p => p.CollectionUrl).IsRequired().HasColumnType("varchar(500)");

    builder.HasMany(p => p.Repositories)
          .WithOne(r => r.Project)
          .HasForeignKey(r => r.ProjectId)
          .OnDelete(DeleteBehavior.Cascade);
  }
}
