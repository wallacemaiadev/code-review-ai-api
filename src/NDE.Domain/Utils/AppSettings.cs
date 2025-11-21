namespace NDE.Domain.Utils;

public sealed class AppSettings
{
  public ModelProvider EmbeddingService { get; set; } = new();
  public ModelProvider LLMService { get; set; } = new();
  public string Portal { get; set; } = string.Empty;
}
