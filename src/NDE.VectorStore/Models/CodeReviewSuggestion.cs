namespace NDE.VectorStore.Models;

public record CodeReviewSuggestion
{
  public Guid CodeReviewId { get; set; }
  public string Fingerprint { get; set; }
  public string RepositoryName { get; set; } = string.Empty;
  public Guid RepositoryId { get; set; }
  public string ProjectName { get; set; } = string.Empty;
  public Guid ProjectId { get; set; }
  public int PullRequestId { get; set; }
  public string File { get; set; } = string.Empty;
  public string Diff { get; set; } = string.Empty;
  public string Suggestion { get; set; } = string.Empty;
  public string? Feedback { get; set; }
  public string Verdict { get; set; }
  public int Weight { get; set; }
  public DateTime CreatedAt { get; set; }

  public CodeReviewSuggestion(Guid codeReviewId, string fingerprint, string repositoryName, Guid repositoryId, string projectName, Guid projectId, int pullRequestId, string file, string diff, string suggestion, string? feedback, string verdict, int weight, DateTime createdAt)
  {
    CodeReviewId = codeReviewId;
    Fingerprint = fingerprint;
    RepositoryName = repositoryName;
    RepositoryId = repositoryId;
    ProjectName = projectName;
    ProjectId = projectId;
    PullRequestId = pullRequestId;
    File = file;
    Diff = diff;
    Suggestion = suggestion;
    Verdict = verdict;
    Feedback = feedback;
    Weight = weight;
    CreatedAt = createdAt;
  }
}
