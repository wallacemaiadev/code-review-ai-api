using Asp.Versioning;

using Mapster;

using Microsoft.AspNetCore.Mvc;

using NDE.Application.Integrations.Azure;
using NDE.Application.Interfaces;
using NDE.Application.ViewModels.Request.CodeReviews;
using NDE.Application.ViewModels.Response.CodeReviews;
using NDE.Domain.Entities.CodeReviews;
using NDE.Domain.Notifications;

namespace NDE.Api.Controllers
{
  [ApiVersion(1)]
  [Route("api/v{v:apiVersion}/[controller]")]
  public class GitController : MainController
  {
    private readonly IProjectService _projectService;
    private readonly IRepositoryService _repositoryService;
    private readonly IAzureIntegration _azureIntegration;

    public GitController(
      IProjectService projectService,
      IRepositoryService repositoryService,
      IAzureIntegration azureIntegration,
      INotificator notificator) : base(notificator)
    {
      _projectService = projectService;
      _repositoryService = repositoryService;
      _azureIntegration = azureIntegration;
    }

    [MapToApiVersion(1)]
    [HttpPost("project/create")]
    public async Task<IActionResult> CreateProject([FromBody] ProjectRequestViewModel request)
    {
      if (!ModelState.IsValid)
        return NDEResponse(ModelState);

      var project = request.Adapt<Project>();

      var projectId = await _projectService.CreateAsync(project);

      return NDEResponse(new ProjectResponseViewModel
      {
        ProjectId = projectId,
        ProjectName = project.Name,
        ProjectUrl = project.Url,
        CollectionUrl = project.CollectionUrl,
      });
    }

    [MapToApiVersion(1)]
    [HttpPost("repository/create")]
    public async Task<IActionResult> CreateRepository([FromBody] RepositoryRequestViewModel request)
    {
      if (!ModelState.IsValid)
        return NDEResponse(ModelState);

      var repository = request.Adapt<Repository>();

      var repositoryId = await _repositoryService.CreateAsync(repository);

      return NDEResponse(new RepositoryResponseViewModel
      {
        RepositoryId = repositoryId,
        RepositoryName = repository.Name,
        RepositoryUrl = repository.Url
      });
    }

    [MapToApiVersion(1)]
    [HttpPost("repository/{id:guid}")]
    public async Task<IActionResult> GetRepositoryById([FromRoute] Guid id)
    {
      if (id.Equals(Guid.Empty))
      {
        NotifyError("Você precisa informar o ID do Repositório.");
        return NDEResponse();
      }

      var repository = await _repositoryService.GetByIdAsync(id);

      return NDEResponse(repository.Adapt<Repository>());
    }

    [MapToApiVersion(1)]
    [HttpPost("project/{id:guid}")]
    public async Task<IActionResult> GetProjectById([FromRoute] Guid id)
    {
      if (id.Equals(Guid.Empty))
      {
        NotifyError("Você precisa informar o ID do Projeto.");
        return NDEResponse();
      }

      var project = await _projectService.GetByIdAsync(id);

      return NDEResponse(project.Adapt<Project>());
    }
  }
}
