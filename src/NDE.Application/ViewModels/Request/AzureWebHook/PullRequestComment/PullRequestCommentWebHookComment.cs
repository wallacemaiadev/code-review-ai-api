namespace NDE.Application.ViewModels.Request.AzureWebHook.PullRequestComment;

using System;
using System.Text.Json.Serialization;

public class PullRequestCommentWebHookComment
{
  [JsonPropertyName("id")]
  public int Id { get; set; }

  [JsonPropertyName("parentCommentId")]
  public int ParentCommentId { get; set; }

  [JsonPropertyName("content")]
  public string Content { get; set; } = string.Empty;

  [JsonPropertyName("publishedDate")]
  public DateTime PublishedDate { get; set; }

  [JsonPropertyName("lastUpdatedDate")]
  public DateTime LastUpdatedDate { get; set; }

  [JsonPropertyName("lastContentUpdatedDate")]
  public DateTime LastContentUpdatedDate { get; set; }

  [JsonPropertyName("commentType")]
  public string CommentType { get; set; } = string.Empty;

  [JsonPropertyName("_links")]
  public PullRequestCommentWebHookLinks Links { get; set; } = new();
}
