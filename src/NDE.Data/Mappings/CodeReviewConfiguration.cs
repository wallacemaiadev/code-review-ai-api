using NDE.Domain.Entities.CodeReviews;

namespace NDE.Data.Mappings;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CodeReviewConfiguration : IEntityTypeConfiguration<CodeReview>
{
  public void Configure(EntityTypeBuilder<CodeReview> builder)
  {
    builder.ToTable("CodeReviews");
    builder.HasKey(cr => cr.Id);

    builder.Property(cr => cr.PullRequestId).IsRequired();
    builder.Property(cr => cr.DiffHash).IsRequired().HasColumnType("text");
    builder.Property(cr => cr.VerdictId).IsRequired().HasConversion<int>();
    builder.Property(cr => cr.FilePath).IsRequired().HasColumnType("varchar(500)");
    builder.Property(cr => cr.FileLink).IsRequired().HasColumnType("varchar(500)");
    builder.Property(cr => cr.Suggestion).IsRequired().HasColumnType("text");
    builder.Property(cr => cr.Diff).HasColumnType("text").IsRequired(true);
    builder.Property(cr => cr.TokensConsumed).IsRequired();
    builder.Property(cr => cr.Language).IsRequired().HasColumnType("varchar(50)");
    builder.Property(cr => cr.Feedback).IsRequired().HasColumnType("text");

    builder.HasMany(cr => cr.Modifications).WithOne(cr => cr.CodeReview).HasForeignKey(fr => fr.CodeReviewId);

    builder.Property(cr => cr.CreatedAt)
        .HasDefaultValueSql("now()")
        .ValueGeneratedOnAdd();

    builder.Property(cr => cr.UpdatedAt)
        .HasDefaultValueSql("now()")
        .ValueGeneratedOnAddOrUpdate();
  }
}
