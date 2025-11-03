using NDE.Domain.Entities.Common;

namespace NDE.Domain.Entities.CodeReviews;

public class AzureProject : Entity<Guid>
{
  public string ProjectName { get; private set; } = string.Empty;
  public string ProjectUrl { get; private set; } = string.Empty;

  public List<AzureRepository> Repositories { get; private set; } = new();
  protected AzureProject() : base(Guid.Empty) { }

  public AzureProject(Guid id, string projectName, string projectUrl) : base(id)
  {
    ProjectName = projectName ?? string.Empty;
    ProjectUrl = projectUrl ?? string.Empty;
  }

  public void AddRepository(AzureRepository repo)
  {
    if (repo is not null) Repositories.Add(repo);
  }
}
