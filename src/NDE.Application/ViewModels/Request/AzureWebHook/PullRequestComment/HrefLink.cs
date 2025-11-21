namespace NDE.Application.ViewModels.Request.AzureWebHook.PullRequestComment;

using System.Text.Json.Serialization;

public class HrefLink
{
  [JsonPropertyName("href")]
  public string Href { get; set; } = string.Empty;
}
