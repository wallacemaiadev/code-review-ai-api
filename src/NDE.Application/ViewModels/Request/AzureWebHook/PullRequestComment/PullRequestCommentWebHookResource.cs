namespace NDE.Application.ViewModels.Request.AzureWebHook.PullRequestComment;

using System.Text.Json.Serialization;

public class PullRequestCommentWebHookResource
{
  [JsonPropertyName("comment")]
  public PullRequestCommentWebHookComment Comment { get; set; } = new();

  [JsonPropertyName("pullRequest")]
  public PullRequestCommentWebHookPullRequest PullRequest { get; set; } = new();
}
