using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using NDE.Domain.Entities.CodeReviews;


namespace NDE.Data.Mappings;

public class RepositoryConfiguration : IEntityTypeConfiguration<Repository>
{
  public void Configure(EntityTypeBuilder<Repository> builder)
  {
    builder.ToTable("Repositories");
    builder.HasKey(r => r.Id);

    builder.Property(r => r.ProjectId).IsRequired();
    builder.Property(r => r.Name).IsRequired().HasColumnType("varchar(200)");
    builder.Property(r => r.Description).IsRequired(false).HasColumnType("text");
    builder.Property(r => r.Url).IsRequired().HasColumnType("varchar(500)");

    builder.HasOne(r => r.Project)
       .WithMany(p => p.Repositories)
       .HasForeignKey(r => r.ProjectId)
       .OnDelete(DeleteBehavior.Cascade);

    builder.HasMany(r => r.PullRequests)
      .WithOne(pr => pr.Repository)
      .HasForeignKey(pr => pr.RepositoryId)
      .OnDelete(DeleteBehavior.Cascade);
  }
}
