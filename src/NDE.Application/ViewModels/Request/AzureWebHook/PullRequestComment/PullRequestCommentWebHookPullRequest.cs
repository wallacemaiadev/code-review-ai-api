namespace NDE.Application.ViewModels.Request.AzureWebHook.PullRequestComment;

using System.Text.Json.Serialization;

public class PullRequestCommentWebHookPullRequest
{
  [JsonPropertyName("pullRequestId")]
  public int PullRequestId { get; set; }

  [JsonPropertyName("repository")]
  public PullRequestCommentWebHookRepository Repository { get; set; } = new();
}
