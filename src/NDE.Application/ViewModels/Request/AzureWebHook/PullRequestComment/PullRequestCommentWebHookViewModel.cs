namespace NDE.Application.ViewModels.Request.AzureWebHook.PullRequestComment;

using System.Text.Json.Serialization;

public class PullRequestCommentWebHookViewModel
{
  [JsonPropertyName("resource")]
  public PullRequestCommentWebHookResource Resource { get; set; } = new();
}
