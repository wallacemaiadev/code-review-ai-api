namespace NDE.Application.ViewModels.Request.Monitoring
{
  public class OtelLogRequest
  {
    public List<OtelResourceLog> ResourceLogs { get; set; } = new();
  }

  public class OtelResourceLog
  {
    public OtelResource Resource { get; set; } = new();
    public List<OtelScopeLog> ScopeLogs { get; set; } = new();
  }

  public class OtelResource
  {
    public List<OtelAttribute> Attributes { get; set; } = new();
  }

  public class OtelScopeLog
  {
    public OtelScope Scope { get; set; } = new();
    public List<OtelLogRecord> LogRecords { get; set; } = new();
  }

  public class OtelScope
  {
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
  }

  public class OtelLogRecord
  {
    public long TimeUnixNano { get; set; }
    public string SeverityText { get; set; } = string.Empty;
    public int SeverityNumber { get; set; }
    public OtelBody Body { get; set; } = new();
    public List<OtelAttribute> Attributes { get; set; } = new();
  }

  public class OtelBody
  {
    public string StringValue { get; set; } = string.Empty;
  }

  public class OtelAttribute
  {
    public string Key { get; set; } = string.Empty;
    public OtelAttributeValue Value { get; set; } = new();
  }

  public class OtelAttributeValue
  {
    public string StringValue { get; set; } = string.Empty;
  }
}
