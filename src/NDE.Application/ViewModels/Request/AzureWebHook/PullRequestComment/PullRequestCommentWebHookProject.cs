namespace NDE.Application.ViewModels.Request.AzureWebHook.PullRequestComment;

using System;
using System.Text.Json.Serialization;

public class PullRequestCommentWebHookProject
{
  [JsonPropertyName("id")]
  public Guid Id { get; set; }

  [JsonPropertyName("name")]
  public string Name { get; set; } = string.Empty;
}
