using Microsoft.Extensions.Logging;

using NDE.Application.Interfaces;
using NDE.Application.Validators;
using NDE.Domain.Entities.CodeReviews;
using NDE.Domain.Interfaces;
using NDE.Domain.Notifications;
using NDE.Observability.Metrics;

namespace NDE.Application.Services;

public class RepositoryService : BaseService, IRepositoryService
{
  private readonly IRepositoryRepository _repositoryRepository;
  private readonly IProjectRepository _projectRepository;
  private readonly ILogger<RepositoryService> _logger;

  public RepositoryService(IRepositoryRepository repositoryRepository, IProjectRepository projectRepository, ILogger<RepositoryService> logger, INotificator notificator) : base(notificator)
  {
    _repositoryRepository = repositoryRepository;
    _projectRepository = projectRepository;
    _logger = logger;
  }

  public async Task<Guid> CreateAsync(Repository repository)
  {
    if (!Validate(new RepositoryValidator(), repository))
    {
      _logger.LogError(
        "Falha ao cadastrar o novo repositório. Projeto: {ProjectId}, Repositório: {RepositoryName}",
        repository?.ProjectId,
        repository?.Name);

      return Guid.Empty;
    }

    var project = await _projectRepository.GetById(repository.ProjectId);
    if (project == null)
    {
      _logger.LogWarning(
        "Projeto não encontrado ao tentar cadastrar o repositório {RepositoryId}",
        repository?.Id);

      return Guid.Empty;
    }

    var repositoryExisting = await _repositoryRepository.GetById(repository.Id);

    if (repositoryExisting == null)
    {
      _logger.LogInformation(
        "Criando novo repositório {RepositoryName} (ID: {RepositoryId}) na API",
        repository.Name,
        repository.Id);

      await _repositoryRepository.Add(repository);

      if (!Commit(await _repositoryRepository.SaveChangesAsync()))
      {
        Notify("Houve um erro ao tentar cadastrar o projeto.");

        _logger.LogError(
            "Falha ao cadastrar projeto {ProjectId} - {ProjectName}",
            project?.Id,
            project?.Name);

        return Guid.Empty;
      }

      AppMetrics.ObserveRepositoryCreated(
        repositoryId: repository.Id,
        repositoryName: repository.Name,
        projectName: project.Name
      );

      return repository.Id;
    }

    return repositoryExisting.Id;
  }

  public async Task<Repository?> GetByIdAsync(Guid id)
  {
    var repository = await _repositoryRepository.GetById(id);
    if (repository == null)
    {
      Notify("O Projeto com o ID informado não foi encontrado.");
      return null;
    }
    return repository;
  }
}