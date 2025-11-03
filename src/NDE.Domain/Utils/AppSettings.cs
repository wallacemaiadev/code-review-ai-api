namespace NDE.Domain.Utils;

public sealed class AppSettings
{
  public bool UseVectorStore { get; set; }
  public OpenAISettings OpenAI { get; set; } = new();
  public string AzureToken { get; set; } = string.Empty;
}


public class OpenAISettings
{
  public string Endpoint { get; set; } = string.Empty;
  public string ApiKey { get; set; } = string.Empty;
  public string ModelName { get; set; } = string.Empty;
  public string DeploymentName { get; set; } = string.Empty;
}