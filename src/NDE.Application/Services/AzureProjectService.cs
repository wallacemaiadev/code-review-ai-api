using Microsoft.Extensions.Logging;

using NDE.Application.Interfaces;
using NDE.Application.Validators;
using NDE.Domain.Entities.CodeReviews;
using NDE.Domain.Interfaces;
using NDE.Domain.Notifications;
using NDE.Observability.Metrics;

namespace NDE.Application.Services;

public class AzureProjectService : BaseService, IAzureProjectService
{
  private readonly IAzureProjectRepository _azureProjectRepository;
  private readonly ILogger<AzureProjectService> _logger;

  public AzureProjectService(IAzureProjectRepository azureProjectRepository, ILogger<AzureProjectService> logger, INotificator notificator) : base(notificator)
  {
    _azureProjectRepository = azureProjectRepository;
    _logger = logger;
  }

  public async Task<Guid> EnsureProjectAsync(AzureProject project)
  {
    if (!Validate(new AzureProjectValidator(), project))
    {
      _logger.LogError("Houve um erro ao tentar processar a solicitação de PR: {@project}", project);
      return Guid.Empty;
    }

    var projectExisting = await _azureProjectRepository.GetProjectByIdAsync(projectId: project.Id);

    if (projectExisting == null)
    {
      await _azureProjectRepository.Add(project);

      if (!Commit(await _azureProjectRepository.SaveChangesAsync()))
      {
        Notify("Houve um erro ao tentar cadastrar o projeto.");
        return Guid.Empty;
      }

      AppMetrics.RecordProjectCreated(
        projectId: project.Id,
        projectName: project.ProjectName
      );

      return project.Id;
    }

    return projectExisting.Id;
  }

  public async Task<AzureProject?> GetProjectByIdAsync(Guid id)
  {
    var project = await _azureProjectRepository.GetById(id);
    if (project == null)
    {
      Notify("O Projeto com o ID informado não foi encontrado.");
      return null;
    }
    return project;
  }
}
