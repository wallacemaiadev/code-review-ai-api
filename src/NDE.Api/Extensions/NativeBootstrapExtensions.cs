using Mapster;

using Microsoft.EntityFrameworkCore;

using NDE.Application.AutoMapper;
using NDE.Application.Interfaces;
using NDE.Application.Services;
using NDE.Application.Workers;
using NDE.Data.Context;
using NDE.Data.Repositories;
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

    services.AddScoped<IPullRequestService, PullRequestService>();
    services.AddScoped<ITeamStandardService, TeamStandardService>();
    services.AddScoped<IProjectService, ProjectService>();
    services.AddScoped<IRepositoryService, RepositoryService>();
    services.AddScoped<IAuthTokenService, AuthTokenService>();
    services.AddScoped<ICodeReviewService, CodeReviewService>();
    services.AddScoped<AuthTokenService>();

    services.AddSingleton<IJobTaskQueue, JobTaskQueue>();
    services.AddSingleton<JobWorkerStore>();
    services.AddHostedService<JobWorker>();

    services.AddScoped<IPromptService>(sp =>
    {
      var env = sp.GetRequiredService<IWebHostEnvironment>();
      var promptPath = Path.Join(env.WebRootPath, "prompts");
      return new PromptService(promptPath);
    });

    services.AddScoped<ILLMService, LLMService>();


    services.AddDistributedMemoryCache();
    services.AddStackExchangeRedisCache(options => { options.Configuration = configuration.GetConnectionString("RedisCacheServer"); options.InstanceName = "nddapi:"; });

    TypeAdapterConfig.GlobalSettings.Scan(typeof(MappingRegistration).Assembly);

    services.AddCors(options =>
    {
      options.AddPolicy("AllowAll", policy =>
      {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
      });
    });
  }

  public static void AddPersistenceService(this IServiceCollection services, IConfiguration configuration)
  {
    services.AddDbContext<AppDbContext>(options =>
    {
      options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
    });

    services.AddScoped<AppDbContext>();
    services.AddScoped<IPullRequestRepository, PullRequestRepository>();
    services.AddScoped<IProjectRepository, ProjectRepository>();
    services.AddScoped<IRepositoryRepository, RepositoryRepository>();
    services.AddScoped<ICodeReviewRepository, CodeReviewRepository>();
  }
}
