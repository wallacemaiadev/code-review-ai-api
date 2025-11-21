using NDE.Domain.Entities.CodeReviews;

namespace NDE.Data.Mappings;

using System.Text.Json;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class CodeModificationConfiguration : IEntityTypeConfiguration<Modification>
{
  public void Configure(EntityTypeBuilder<Modification> builder)
  {
    builder.ToTable("CodeModifications");
    builder.HasKey(cr => cr.Id);

    builder.Property(cr => cr.CodeBlock).IsRequired().HasColumnType("text");

    builder.Property(cr => cr.Vector)
    .HasColumnType("text")
    .IsRequired(false)
    .HasConversion(
        v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
        v => JsonSerializer.Deserialize<float[]>(v, (JsonSerializerOptions?)null) ?? Array.Empty<float>())
    .Metadata.SetValueComparer(EfValueComparers.ArrayOfFloat);
  }
}
