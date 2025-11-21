using Microsoft.Extensions.Logging;

using NDE.Application.Interfaces;
using NDE.Application.Validators;
using NDE.Domain.Entities.CodeReviews;
using NDE.Domain.Interfaces;
using NDE.Domain.Notifications;
using NDE.Observability.Metrics;

namespace NDE.Application.Services;

public class ProjectService : BaseService, IProjectService
{
  private readonly IProjectRepository _projectRepository;
  private readonly ILogger<ProjectService> _logger;

  public ProjectService(IProjectRepository projectRepository, ILogger<ProjectService> logger, INotificator notificator) : base(notificator)
  {
    _projectRepository = projectRepository;
    _logger = logger;
  }

  public async Task<Guid> CreateAsync(Project project)
  {
    if (!Validate(new ProjectValidator(), project))
    {
      _logger.LogError("Houve um erro ao tentar processar a solicitação de PR: {@project}", project);
      return Guid.Empty;
    }

    var projectExisting = await _projectRepository.GetById(id: project.Id);

    if (projectExisting == null)
    {
      await _projectRepository.Add(project);

      if (!Commit(await _projectRepository.SaveChangesAsync()))
      {
        Notify("Houve um erro ao tentar cadastrar o projeto.");
        return Guid.Empty;
      }

      AppMetrics.ObserveProjectCreated(
        projectId: project.Id,
        projectName: project.Name
      );

      return project.Id;
    }

    return projectExisting.Id;
  }

  public async Task<Project?> GetByIdAsync(Guid id)
  {
    var project = await _projectRepository.GetById(id);
    if (project == null)
    {
      Notify("O Projeto com o ID informado não foi encontrado.");
      return null;
    }
    return project;
  }
}
