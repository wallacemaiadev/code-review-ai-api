using NDE.Domain.Entities.Common;

namespace NDE.Domain.Entities.CodeReviews;

public class Repository : Entity<Guid>
{
  public Guid ProjectId { get; private set; }
  public Project Project { get; private set; } = default!;

  public string Name { get; private set; } = string.Empty;
  public string Description { get; private set; } = string.Empty;
  public string Url { get; private set; } = string.Empty;

  public List<PullRequest> PullRequests { get; private set; } = new();

  protected Repository() : base(Guid.Empty) { }

  public Repository(Guid id, Guid projectId, string name, string description, string url) : base(id)
  {
    ProjectId = projectId;
    Name = name;
    Description = description;
    Url = url;
  }
}
