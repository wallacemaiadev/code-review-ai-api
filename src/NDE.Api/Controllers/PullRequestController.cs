using Asp.Versioning;

using Microsoft.AspNetCore.Mvc;

using NDE.Application.Integrations.Azure;
using NDE.Application.Interfaces;
using NDE.Application.ViewModels.Request.CodeReviews;
using NDE.Domain.Notifications;

namespace NDE.Api.Controllers
{
  [ApiVersion(1)]
  [Route("api/v{v:apiVersion}/[controller]")]
  public class PullRequestController : MainController
  {
    private readonly IPullRequestService _pullRequestService;
    private readonly IAzureIntegration _azureIntegration;

    public PullRequestController(IPullRequestService pullRequestService, IAzureIntegration azureIntegration, INotificator notificator) : base(notificator)
    {
      _pullRequestService = pullRequestService;
      _azureIntegration = azureIntegration;
    }

    [MapToApiVersion(1)]
    [HttpPost("info")]
    public async Task<IActionResult> GetPullRequestInfo([FromBody] AzurePullRequestRequestViewModel request)
    {
      return NDEResponse
        (
          await _azureIntegration.GetPullRequestInfoAsync
          (
            collectionUrl: request.CollectionUrl,
            projectName: request.ProjectName,
            repositoryId: request.RepositoryId,
            token: request.AuthToken,
            pullRequestId: request.PullRequestId
          )
        );
    }
  }
}
