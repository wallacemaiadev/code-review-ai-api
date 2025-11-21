namespace NDE.VectorStore.Models;

public record CodeReviewSuggestion
{
  public Guid CodeModificationId { get; set; }
  public Guid CodeReviewId { get; set; }
  public Guid RepositoryId { get; set; }
  public Guid ProjectId { get; set; }
  public int PullRequestId { get; set; }
  public string FilePath { get; set; } = string.Empty;
  public string CodeBlock { get; set; } = string.Empty;
  public string Suggestion { get; set; } = string.Empty;
  public string? Feedback { get; set; }
  public string Verdict { get; set; }
  public int Weight { get; set; }
  public DateTime CreatedAt { get; set; }

  public CodeReviewSuggestion(
    Guid codeReviewId,
    Guid codeModificationId,
    Guid repositoryId,
    Guid projectId,
    int pullRequestId,
    string filePath,
    string codeBlock,
    string suggestion,
    string? feedback,
    string verdict,
    int weight,
    DateTime createdAt)
  {
    CodeReviewId = codeReviewId;
    CodeModificationId = codeModificationId;
    RepositoryId = repositoryId;
    ProjectId = projectId;
    PullRequestId = pullRequestId;
    FilePath = filePath;
    CodeBlock = codeBlock;
    Suggestion = suggestion;
    Verdict = verdict;
    Feedback = feedback;
    Weight = weight;
    CreatedAt = createdAt;
  }
}
