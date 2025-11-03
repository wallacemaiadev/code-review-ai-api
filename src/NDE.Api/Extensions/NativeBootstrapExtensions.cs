using Microsoft.EntityFrameworkCore;

using NDE.Application.AutoMapper;
using NDE.Application.Interfaces;
using NDE.Application.Services;
using NDE.Data.Context;
using NDE.Data.Repository;
using NDE.Domain.Interfaces;
using NDE.Domain.Notifications;
using NDE.Domain.Utils;

namespace NDE.Api.Extensions;

public static class NativeBootstrapExtensions
{
  public static void AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
  {
    if (services is null) throw new ArgumentNullException(nameof(services));
    if (configuration is null) throw new ArgumentNullException(nameof(configuration));

    services.Configure<AppSettings>(configuration.GetSection("AppSettings"));
    services.Configure<AppSettings>(configuration);

    services.AddScoped<INotificator, Notificator>();
    services.AddScoped<IEmbeddingService, EmbeddingService>();

    services.AddScoped<IAzurePullRequestService, AzurePullRequestService>();
    services.AddScoped<ITeamStandardService, TeamStandardService>();
    services.AddScoped<IAzureProjectService, AzureProjectService>();
    services.AddScoped<IAzureRepositoryService, AzureRepositoryService>();

    services.AddDistributedMemoryCache();
    services.AddStackExchangeRedisCache(options => { options.Configuration = configuration.GetConnectionString("RedisCacheServer"); options.InstanceName = "nddapi:"; });

    services.AddAutoMapper(cfg => { }, typeof(CodeReviewProfile));
  }

  public static void AddPersistenceService(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddDbContext<AppDbContext>(options =>
    {
      options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
    });

    services.AddScoped<AppDbContext>();
    services.AddScoped<IAzurePullRequestRepository, AzurePullRequestRepository>();
    services.AddScoped<IAzureProjectRepository, AzureProjectRepository>();
    services.AddScoped<IAzureRepositoryRepository, AzureRepositoryRepository>();
    services.AddScoped<ICodeReviewRepository, CodeReviewRepository>();
  }
}
