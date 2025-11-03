namespace NDE.Api.Middleware
{
  public static class ApiKeyMiddlewareExtensions
  {
    public static IApplicationBuilder UseApiKeyAuth(this IApplicationBuilder builder)
    {
      return builder.UseMiddleware<ApiKeyMiddleware>();
    }
  }
}

