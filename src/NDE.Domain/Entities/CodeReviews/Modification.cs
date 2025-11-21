using NDE.Domain.Entities.Common;

namespace NDE.Domain.Entities.CodeReviews;

public class Modification : Entity<Guid>
{
  public Guid CodeReviewId { get; private set; }
  public string CodeBlock { get; private set; } = string.Empty;
  public float[] Vector { get; private set; } = Array.Empty<float>();

  public CodeReview CodeReview { get; private set; } = default!;

  protected Modification() : base(Guid.Empty) { }

  public Modification(Guid codeReviewId, string codeBlock) : base(Guid.NewGuid())
  {
    CodeReviewId = codeReviewId;
    CodeBlock = codeBlock;
  }

  public void SetVector(float[]? vector)
  {
    if (vector == null || vector.Length == 0)
      throw new ArgumentNullException("Vector não pode ser nulo ou vazio.", nameof(vector));

    Vector = vector;
  }
}