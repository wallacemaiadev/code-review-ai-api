using Asp.Versioning;

using Microsoft.AspNetCore.Mvc;

using NDE.Application.ViewModels.Request.Monitoring;
using NDE.Domain.Notifications;

namespace NDE.Api.Controllers;

[ApiVersion(1)]
[Route("api/v{v:apiVersion}/[controller]")]
public class MonitoringController : MainController
{
  private ILogger<MonitoringController> _logger;
  public MonitoringController(ILogger<MonitoringController> logger, INotificator notificator) : base(notificator)
  {
    _logger = logger;
  }

  [MapToApiVersion("1")]
  [HttpPost("logs")]
  public IActionResult SendLogs([FromBody] OtelLogRequest request)
  {
    if (!ModelState.IsValid)
      return BadRequest(ModelState);

    if (request?.ResourceLogs == null || request.ResourceLogs.Count == 0)
      return Ok();

    foreach (var resourceLog in request.ResourceLogs)
    {
      if (resourceLog.ScopeLogs == null)
        continue;

      foreach (var scopeLog in resourceLog.ScopeLogs)
      {
        if (scopeLog.LogRecords == null)
          continue;

        foreach (var record in scopeLog.LogRecords)
        {
          var level = MapSeverity(record.SeverityText, record.SeverityNumber);
          var message = record.Body?.StringValue ?? string.Empty;

          var attributes = new Dictionary<string, object>();

          if (record.Attributes != null)
          {
            foreach (var attr in record.Attributes)
            {
              if (!string.IsNullOrWhiteSpace(attr.Key))
                attributes[attr.Key] = attr.Value?.StringValue ?? string.Empty;
            }
          }

          if (record.TimeUnixNano > 0)
          {
            var timestamp = DateTimeOffset.FromUnixTimeMilliseconds(record.TimeUnixNano / 1_000_000L);
            attributes["remoteTimestamp"] = timestamp;
          }

          using (_logger.BeginScope(attributes))
          {
            _logger.Log(level, message);
          }
        }
      }
    }

    return Ok();
  }

  private static LogLevel MapSeverity(string severityText, int severityNumber)
  {
    if (!string.IsNullOrWhiteSpace(severityText))
    {
      switch (severityText.ToUpperInvariant())
      {
        case "DEBUG":
          return LogLevel.Debug;
        case "INFO":
          return LogLevel.Information;
        case "WARN":
        case "WARNING":
          return LogLevel.Warning;
        case "ERROR":
          return LogLevel.Error;
      }
    }

    if (severityNumber >= 17)
      return LogLevel.Error;
    if (severityNumber >= 13)
      return LogLevel.Warning;
    if (severityNumber >= 9)
      return LogLevel.Information;
    if (severityNumber > 0)
      return LogLevel.Debug;

    return LogLevel.Information;
  }
}