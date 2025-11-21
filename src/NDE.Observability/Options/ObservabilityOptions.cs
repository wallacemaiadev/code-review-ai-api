namespace NDE.Observability.Options;

public class ObservabilityOptions
{
  public string ServiceName { get; set; } = string.Empty;
  public string CollectorUrl { get; set; } = string.Empty;
  public Uri CollectorUri => new(CollectorUrl);
  public string Version { get; set; } = string.Empty;
}