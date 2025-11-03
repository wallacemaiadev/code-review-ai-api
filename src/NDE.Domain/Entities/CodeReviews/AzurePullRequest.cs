using NDE.Domain.Entities.Common;

namespace NDE.Domain.Entities.CodeReviews;

public class AzurePullRequest : Entity<int>
{
  public Guid RepositoryId { get; private set; }
  public AzureRepository AzureRepository { get; private set; } = default!;

  public DateTime CreatedAt { get; private set; }

  public List<CodeReview> CodeReviews { get; set; } = new();

  protected AzurePullRequest() : base(0) { }

  public AzurePullRequest(int id, Guid repositoryId) : base(id)
  {
    RepositoryId = repositoryId;
    CreatedAt = DateTime.UtcNow;
  }
}
