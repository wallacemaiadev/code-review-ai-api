using NDE.Domain.Entities.Common;

namespace NDE.Domain.Entities.CodeReviews;

public class AzureRepository : Entity<Guid>
{
  public Guid ProjectId { get; private set; }
  public AzureProject AzureProject { get; private set; } = default!;

  public string RepositoryName { get; private set; } = string.Empty;
  public string RepositoryUrl { get; private set; } = string.Empty;

  public List<AzurePullRequest> PullRequests { get; private set; } = new();

  protected AzureRepository() : base(Guid.Empty) { }

  public AzureRepository(Guid id, Guid projectId, string repositoryName, string repositoryUrl) : base(id)
  {
    ProjectId = projectId;
    RepositoryName = repositoryName ?? string.Empty;
    RepositoryUrl = repositoryUrl ?? string.Empty;
  }

  public void SetRepositoryUrl(string url) => RepositoryUrl = url ?? string.Empty;
  public void AddPullRequest(AzurePullRequest pr) { if (pr is not null) PullRequests.Add(pr); }
}
