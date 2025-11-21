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
  private readonly IRepositoryRepository _repositoryRepository;
  private readonly IProjectRepository _ProjectRepository;
  private readonly IDistributedCache _cache;
  private readonly ILogger<TeamStandardService> _logger;

  public TeamStandardService(
    IDistributedCache cache,
    IRepositoryRepository RepositoryRepository,
    IProjectRepository ProjectRepository,
    ILogger<TeamStandardService> logger,
    INotificator notificator) : base(notificator)
  {
    _repositoryRepository = RepositoryRepository;
    _ProjectRepository = ProjectRepository;
    _cache = cache;
    _logger = logger;
  }

  public async Task<bool> SaveTeamStandardAsync(string teamStandard, Guid repositoryId)
  {
    if (teamStandard is null)
    {
      Notify("Payload do TeamStandard inválido.");
      return false;
    }

    var repos = await _repositoryRepository.GetById(
      id: repositoryId);

    if (repos is null)
    {
      Notify("Repositório não encontrado.");
      return false;
    }

    var project = await _ProjectRepository.GetById(
      id: repos.ProjectId);

    if (project is null)
    {
      Notify("Projeto não encontrado.");
      return false;
    }

    var cacheKey = $"TeamStandard:{repos.Id}:{project.Id}";

    var options = new DistributedCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromDays(10))
                .SetSlidingExpiration(TimeSpan.FromDays(15));

    await _cache.SetAsync(cacheKey, teamStandard, options);

    AppMetrics.ObserveTeamStandardCreated(
      repositoryId: repos.Id,
      repositoryName: repos.Name,
      projectName: project.Name);

    return true;
  }

  public async Task<string?> GetTeamStandardAsync(Guid repositoryId)
  {
    var repos = await _repositoryRepository.GetById(
      id: repositoryId);

    if (repos is null)
    {
      Notify("Repositório não encontrado.");
      return null;
    }

    var project = await _ProjectRepository.GetById(
      id: repos.ProjectId);

    if (project is null)
    {
      Notify("Projeto não encontrado.");
      return null;
    }

    var cacheKey = $"TeamStandard:{repos.Id}:{project.Id}";

    if (!_cache.TryGetValue(cacheKey, out string? teamStandard))
      return null;

    return teamStandard;
  }
}
