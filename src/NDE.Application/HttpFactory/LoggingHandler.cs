using Microsoft.Extensions.Logging;

namespace NDE.Application.HttpFactory
{
  public class LoggingHandler : DelegatingHandler
  {
    private readonly ILogger<LoggingHandler> _logger;

    public LoggingHandler(ILogger<LoggingHandler> logger)
    {
      _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
      _logger.LogInformation("Request: {Method} {Url}", request.Method, request.RequestUri);

      if (request.Content != null)
      {
        var content = await request.Content.ReadAsStringAsync(cancellationToken);
        if (!string.IsNullOrWhiteSpace(content))
          _logger.LogDebug("Request Body: {Body}", content);
      }

      var response = await base.SendAsync(request, cancellationToken);

      _logger.LogInformation("Response: {StatusCode} {Url}", response.StatusCode, request.RequestUri);

      if (response.Content != null)
      {
        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!string.IsNullOrWhiteSpace(responseContent))
          _logger.LogDebug("Response Body: {Body}", responseContent);
      }

      return response;
    }
  }
}