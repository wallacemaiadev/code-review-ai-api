using Asp.Versioning;

using AutoMapper;

using Microsoft.AspNetCore.Mvc;

using NDE.Application.Interfaces;
using NDE.Application.ViewModels.Request.CodeReviews;
using NDE.Domain.Entities.CodeReviews;
using NDE.Domain.Notifications;

namespace NDE.Api.Controllers
{
  [ApiVersion(1)]
  [Route("api/v{v:apiVersion}/[controller]")]
  public class AzureController : MainController
  {
    private readonly IAzureProjectService _azureProjectService;
    private readonly IAzureRepositoryService _azureRepositoryService;
    private readonly IMapper _mapper;

    public AzureController(IAzureProjectService azureProjectService, IAzureRepositoryService azureRepositoryService, IMapper mapper, INotificator notificator) : base(notificator)
    {
      _azureProjectService = azureProjectService;
      _azureRepositoryService = azureRepositoryService;
      _mapper = mapper;
    }

    [MapToApiVersion(1)]
    [HttpPost("project/create")]
    public async Task<IActionResult> CreateProject([FromBody] ProjectRequestViewModel request)
    {
      if (!ModelState.IsValid)
        return NDEResponse(ModelState);

      var project = _mapper.Map<AzureProject>(request);

      var projectId = await _azureProjectService.EnsureProjectAsync(project);

      return NDEResponse(new
      {
        ProjectId = projectId,
        ProjectName = project.ProjectName,
        ProjectUrl = project.ProjectUrl
      });
    }

    [MapToApiVersion(1)]
    [HttpPost("repository/create")]
    public async Task<IActionResult> CreateRepository([FromBody] RepositoryRequestViewModel request)
    {
      if (!ModelState.IsValid)
        return NDEResponse(ModelState);

      var repository = _mapper.Map<AzureRepository>(request);

      var repositoryId = await _azureRepositoryService.EnsureRepositoryAsync(repository);

      return NDEResponse(new
      {
        RepositoryId = repositoryId,
        RepositoryName = repository.RepositoryName,
        RepositoryUrl = repository.RepositoryUrl
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

      return NDEResponse(_mapper.Map<AzureRepository>(await _azureRepositoryService.GetRepositoryByIdAsync(id)));
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

      return NDEResponse(_mapper.Map<AzureProject>(await _azureProjectService.GetProjectByIdAsync(id)));
    }
  }
}
