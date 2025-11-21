using NDE.Domain.Entities.Common;

namespace NDE.Domain.Entities.CodeReviews;

public class Project : Entity<Guid>
{
  public string Name { get; private set; } = string.Empty;
  public string Description { get; private set; } = string.Empty;
  public string Url { get; private set; } = string.Empty;
  public string CollectionUrl { get; set; } = string.Empty;
  public List<Repository> Repositories { get; private set; } = new();

  protected Project() : base(Guid.Empty) { }

  public Project(Guid id, string name, string description, string url, string collectionUrl) : base(id)
  {
    Name = name;
    Url = url;
    CollectionUrl = collectionUrl;
    Description = description;
  }
}
