using Asp.Versioning;

namespace NDE.Api.Extensions;

public static class VersioningApiExtensions
{
  public static void AddApiVersioningService(this IServiceCollection services)
  {
    if (services is null) throw new ArgumentNullException(nameof(services));

    services.AddApiVersioning(options =>
    {
      options.DefaultApiVersion = new ApiVersion(1);
      options.ReportApiVersions = true;
      options.AssumeDefaultVersionWhenUnspecified = true;
      options.ApiVersionReader = ApiVersionReader.Combine(
          new UrlSegmentApiVersionReader(),
          new HeaderApiVersionReader("X-Api-Version"));
    })
    .AddMvc()
    .AddApiExplorer(options =>
    {
      options.GroupNameFormat = "'v'V";
      options.SubstituteApiVersionInUrl = true;
    });
  }
}
