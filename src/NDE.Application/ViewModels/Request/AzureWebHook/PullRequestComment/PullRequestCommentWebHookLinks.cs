namespace NDE.Application.ViewModels.Request.AzureWebHook.PullRequestComment;

using System.Text.Json.Serialization;

public class PullRequestCommentWebHookLinks
{
  [JsonPropertyName("self")]
  public HrefLink Self { get; set; } = new();

  [JsonPropertyName("repository")]
  public HrefLink Repository { get; set; } = new();

  [JsonPropertyName("threads")]
  public HrefLink Threads { get; set; } = new();
}
