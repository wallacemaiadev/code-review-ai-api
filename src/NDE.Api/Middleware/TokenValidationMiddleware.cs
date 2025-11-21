using NDE.Application.Services;

namespace NDE.Api.Middleware
{
  public class TokenValidationMiddleware
  {
    private readonly RequestDelegate _next;
    private readonly ILogger<TokenValidationMiddleware> _logger;

    public TokenValidationMiddleware(RequestDelegate next, ILogger<TokenValidationMiddleware> logger)
    {
      _next = next;
      _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, AuthTokenService authTokenService)
    {
      if (!context.Request.Query.ContainsKey("token"))
      {
        await _next(context);
        return;
      }

      var tokenData = await authTokenService.ValidateFromQueryStringAsync(context.Request.QueryString.Value);

      if (tokenData == null)
      {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync("{\"error\":\"Token inválido ou expirado\"}");

        _logger.LogWarning(
          "Tentativa de acesso negada. Motivo: Token inválido ou expirado. Path: {Path}, IP: {IP}",
          context.Request.Path,
          context.Connection.RemoteIpAddress);
        return;
      }

      context.Items["TokenData"] = tokenData;

      _logger.LogInformation(
        "Token validado com sucesso. Path: {Path}, IP: {IP}, ExpiresAt: {ExpiresAt}",
        context.Request.Path,
        context.Connection.RemoteIpAddress,
        tokenData.ExpiresAt);

      await _next(context);
    }
  }
}