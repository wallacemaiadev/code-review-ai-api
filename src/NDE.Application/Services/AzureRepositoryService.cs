using Microsoft.Extensions.Logging;

using NDE.Application.Interfaces;
using NDE.Application.Validators;
using NDE.Domain.Entities.CodeReviews;
using NDE.Domain.Interfaces;
using NDE.Domain.Notifications;
using NDE.Observability.Metrics;

namespace NDE.Application.Services;

public class AzureRepositoryService : BaseService, IAzureRepositoryService
{
  private readonly IAzureRepositoryRepository _azureRepositoryRepository;
  private readonly IAzureProjectRepository _azureProjectRepository;
  private readonly ILogger<AzureRepositoryService> _logger;

  public AzureRepositoryService(IAzureRepositoryRepository azureRepositoryRepository, IAzureProjectRepository azureProjectRepository, ILogger<AzureRepositoryService> logger, INotificator notificator) : base(notificator)
  {
    _azureRepositoryRepository = azureRepositoryRepository;
    _azureProjectRepository = azureProjectRepository;
    _logger = logger;
  }

  public async Task<Guid> EnsureRepositoryAsync(AzureRepository repository)
  {
    if (!Validate(new AzureRepositoryValidator(), repository))
    {
      _logger.LogError(
        "Falha ao cadastrar o novo repositório. Projeto: {ProjectId}, Repositório: {RepositoryName}",
        repository?.ProjectId,
        repository?.RepositoryName);

      return Guid.Empty;
    }

    var project = await _azureProjectRepository.GetById(repository.ProjectId);
    if (project == null)
    {
      _logger.LogWarning(
        "Projeto não encontrado ao tentar cadastrar o repositório {RepositoryId}",
        repository?.Id);

      return Guid.Empty;
    }

    var repositoryExisting = await _azureRepositoryRepository.GetRepositoryByIdAsync(repositoryId: repository.Id);

    if (repositoryExisting == null)
    {
      _logger.LogInformation(
        "Criando novo repositório {RepositoryName} (ID: {RepositoryId}) na API",
        repository.RepositoryName,
        repository.Id);

      await _azureRepositoryRepository.Add(repository);

      if (!Commit(await _azureRepositoryRepository.SaveChangesAsync()))
      {
        Notify("Houve um erro ao tentar cadastrar o projeto.");

        _logger.LogError(
            "Falha ao cadastrar projeto {ProjectId} - {ProjectName}",
            project?.Id,
            project?.ProjectName);

        return Guid.Empty;
      }

      AppMetrics.RecordRepositoryCreated(
        repositoryId: repository.Id,
        repositoryName: repository.RepositoryName,
        projectName: project.ProjectName
      );

      return repository.Id;
    }

    return repositoryExisting.Id;
  }

  public async Task<AzureRepository?> GetRepositoryByIdAsync(Guid id)
  {
    var repository = await _azureRepositoryRepository.GetById(id);
    if (repository == null)
    {
      Notify("O Projeto com o ID informado não foi encontrado.");
      return null;
    }
    return repository;
  }
}