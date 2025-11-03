using Asp.Versioning;

using Microsoft.AspNetCore.Mvc;

using NDE.Application.ViewModels.Request.AzureWebHook;
using NDE.Domain.Notifications;
using NDE.Observability.Metrics;

namespace NDE.Api.Controllers;

[ApiVersion(1)]
[Route("api/v{v:apiVersion}/[controller]")]
public class PipelineController : MainController
{
  private ILogger<PipelineController> _logger;
  public PipelineController(ILogger<PipelineController> logger, INotificator notificator) : base(notificator)
  {
    _logger = logger;
  }

  [MapToApiVersion(1)]
  [HttpPost("monitoring/build")]
  public IActionResult MonitoringBuild([FromBody] BuildCompleteRequestViewModel request)
  {
    if (!ModelState.IsValid)
      return NDEResponse(ModelState);

    _logger.LogInformation(
        "Recebido evento de build {BuildId} ({BuildNumber}) do projeto {ProjectName} no repositório {RepositoryName}. Status: {Status}, Resultado: {Result}",
        request.Resource.Id,
        request.Resource.BuildNumber,
        request.Resource.Definition.Project.Name,
        request.Resource.Repository.Name,
        request.Resource.Status,
        request.Resource.Result);

    AppMetrics.RecordBuildPipeline(
        buildId: request.Resource.Id,
        buildNumber: request.Resource.BuildNumber,
        queueTime: request.Resource.QueueTime,
        startTime: request.Resource.StartTime,
        finishTime: request.Resource.FinishTime,
        sourceBranch: request.Resource.SourceBranch,
        repositoryName: request.Resource.Repository.Name,
        projectName: request.Resource.Definition.Project.Name,
        repositoryId: request.Resource.Repository.Id,
        projectId: request.Resource.Definition.Project.Id,
        status: request.Resource.Status,
        result: request.Resource.Result
    );

    return NDEResponse(
    $"Evento de build {request.Resource.Id} ({request.Resource.BuildNumber}) registrado com sucesso!");
  }
}