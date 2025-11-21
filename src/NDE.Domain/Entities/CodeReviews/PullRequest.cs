using NDE.Domain.Entities.Common;

namespace NDE.Domain.Entities.CodeReviews;

public class PullRequest : Entity<Guid>
{
  public int PullRequestId { get; private set; }
  public Guid RepositoryId { get; private set; }
  public string Title { get; private set; } = string.Empty;
  public string Description { get; private set; } = string.Empty;
  public int TokensConsumed { get; private set; }
  public DateTime CreatedAt { get; private set; }
  public DateTime UpdatedAt { get; private set; }

  public Repository Repository { get; private set; } = default!;

  public List<CodeReview> CodeReviews { get; private set; } = new();

  protected PullRequest() : base(Guid.Empty) { }

  public PullRequest(int pullRequestId, Guid repositoryId, string title, string description) : base(Guid.NewGuid())
  {
    Title = title;
    Description = description;
    PullRequestId = pullRequestId;
    RepositoryId = repositoryId;
    CreatedAt = DateTime.UtcNow;
    UpdatedAt = DateTime.UtcNow;
  }

  public bool CheckUpdated(string title, string description)
  {
    if (string.IsNullOrWhiteSpace(title))
      throw new ArgumentNullException("O titulo não pode ser nulo ou vazio.", nameof(title));

    if (string.IsNullOrWhiteSpace(description))
      throw new ArgumentNullException("A descrição não pode ser nulo ou vazio.", nameof(description));

    bool updated = false;

    if (Title != title)
    {
      Title = title;
      updated = Updated();
    }

    if (Description != description)
    {
      Description = description;
      updated = Updated();
    }

    return updated;
  }

  public int SetTokens(int tokens)
  {
    if (tokens > 0)
    {
      TokensConsumed += tokens;
      Updated();
    }

    return TokensConsumed;
  }

  public bool Updated()
  {
    UpdatedAt = DateTime.UtcNow;
    return true;
  }
}
