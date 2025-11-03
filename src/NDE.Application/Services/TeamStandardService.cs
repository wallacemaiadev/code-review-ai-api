using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

using NDE.Application.Interfaces;
using NDE.Domain.Caching;
using NDE.Domain.Interfaces;
using NDE.Domain.Notifications;
using NDE.Observability.Metrics;

namespace NDE.Application.Services;

public class TeamStandardService : BaseService, ITeamStandardService
{
  private readonly IAzureRepositoryRepository _azureRepositoryRepository;
  private readonly IAzureProjectRepository _azureProjectRepository;
  private readonly IDistributedCache _cache;
  private readonly ILogger<TeamStandardService> _logger;

  public TeamStandardService(
    IDistributedCache cache,
    IAzureRepositoryRepository azureRepositoryRepository,
    IAzureProjectRepository azureProjectRepository,
    ILogger<TeamStandardService> logger,
    INotificator notificator) : base(notificator)
  {
    _azureRepositoryRepository = azureRepositoryRepository;
    _azureProjectRepository = azureProjectRepository;
    _cache = cache;
    _logger = logger;
  }

  public async Task<bool> SaveTeamStandardAsync(object teamStandard, Guid repositoryId)
  {
    if (teamStandard is null)
    {
      Notify("Payload do TeamStandard inválido.");
      return false;
    }

    var repos = await _azureRepositoryRepository.GetRepositoryByIdAsync(
      repositoryId: repositoryId);

    if (repos is null)
    {
      Notify("Repositório não encontrado.");
      return false;
    }

    var project = await _azureProjectRepository.GetProjectByIdAsync(
      projectId: repos.ProjectId);

    if (project is null)
    {
      Notify("Projeto não encontrado.");
      return false;
    }

    var cacheKey = $"TeamStandard:{repos.Id}";

    var options = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromDays(10))
                .SetSlidingExpiration(TimeSpan.FromDays(15));

    await _cache.SetAsync(cacheKey, teamStandard, options);

    AppMetrics.RecordTeamStandardCreated(
      repositoryId: repos.Id,
      repositoryName: repos.RepositoryName,
      projectName: project.ProjectName);

    return true;
  }

  public async Task<object?> GetTeamStandardAsync(Guid repositoryId)
  {
    var repos = await _azureRepositoryRepository.GetRepositoryByIdAsync(
      repositoryId: repositoryId);

    if (repos is null)
    {
      Notify("Repositório não encontrado.");
      return false;
    }

    var project = await _azureProjectRepository.GetProjectByIdAsync(
      projectId: repos.ProjectId);

    if (project is null)
    {
      Notify("Projeto não encontrado.");
      return false;
    }

    var cacheKey = $"TeamStandard:{repos.Id}";

    if (!_cache.TryGetValue(cacheKey, out object? teamStandard))
      return null;

    return teamStandard;
  }
}
