namespace NDE.Api.Middleware;

public class AuthMiddleware
{
  private readonly RequestDelegate _next;
  private const string HeaderName = "api-key";
  private readonly ILogger<AuthMiddleware> _logger;
  public AuthMiddleware(RequestDelegate next, ILogger<AuthMiddleware> logger)
  {
    _next = next;
    _logger = logger;
  }

  public async Task InvokeAsync(HttpContext context, IConfiguration configuration, IHostEnvironment env)
  {
    if (env.IsDevelopment())
    {
      await _next(context);
      return;
    }

    if (!context.Request.Headers.TryGetValue(HeaderName, out var extractedApiKey))
    {
      context.Response.StatusCode = StatusCodes.Status401Unauthorized;
      await context.Response.WriteAsync("API Key não encontrada.");

      _logger.LogWarning(
          "Tentativa de acesso negada. Motivo: API Key não encontrada. Path: {Path}, IP: {IP}",
          context.Request.Path,
          context.Connection.RemoteIpAddress);

      return;
    }

    var extractedApiKeyString = extractedApiKey.ToString().Trim().Trim('"');
    var configuredApiKey = configuration["ApiKey"]?.Trim();

    if (!string.Equals(extractedApiKeyString?.Trim(), configuredApiKey?.Trim(), StringComparison.Ordinal))
    {
      context.Response.StatusCode = StatusCodes.Status403Forbidden;
      await context.Response.WriteAsync("API Key inválida.");

      _logger.LogWarning(
          "Tentativa de acesso negada. Motivo: API Key inválida. Path: {Path}, IP: {IP}",
          context.Request.Path,
          context.Connection.RemoteIpAddress);

      return;
    }

    await _next(context);
  }
}
