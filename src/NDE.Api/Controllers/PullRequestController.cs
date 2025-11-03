using Asp.Versioning;

using AutoMapper;

using Microsoft.AspNetCore.Mvc;

using NDE.Application.Interfaces;
using NDE.Application.ViewModels.Request.AzureWebHook.PullRequestComment;
using NDE.Application.ViewModels.Request.CodeReviews;
using NDE.Domain.Notifications;

namespace NDE.Api.Controllers
{
  [ApiVersion(1)]
  [Route("api/v{v:apiVersion}/[controller]")]
  public class PullRequestController : MainController
  {
    private readonly IAzurePullRequestService _pullRequestService;
    private readonly IMapper _mapper;

    public PullRequestController(IAzurePullRequestService pullRequestService, IMapper mapper, INotificator notificator) : base(notificator)
    {
      _pullRequestService = pullRequestService;
      _mapper = mapper;
    }

    [MapToApiVersion(1)]
    [HttpPost("review/request")]
    public async Task<IActionResult> ReviewRequest([FromBody] ReviewRequestViewModel request)
    {
      if (!ModelState.IsValid)
        return NDEResponse(ModelState);

      return NDEResponse(await _pullRequestService.ReviewRequestAsync(request));
    }

    [MapToApiVersion(1)]
    [HttpPost("review/save")]
    public async Task<IActionResult> SaveReview([FromBody] SaveReviewRequestViewModel request)
    {
      if (!ModelState.IsValid)
        return NDEResponse(ModelState);

      return NDEResponse(await _pullRequestService.SaveReviewAsync(request));
    }

    [MapToApiVersion(1)]
    [HttpPost("review/request")]
    public async Task<IActionResult> PreviousSuggestions([FromQuery] PreviousSuggestionsRequestViewModel request)
    {
      return NDEResponse(await _pullRequestService.GetPreviousSuggestions(repositoryId: request.RepositoryId, pullRequestId: request.PullRequestId));
    }
    
    [MapToApiVersion(1)]
    [HttpPost("webhook/comment-update")]
    public async Task<IActionResult> WHCommentUpdate(PullRequestCommentWebHookViewModel request)
    {
      return NDEResponse(await _pullRequestService.HandleReviewAsync(
        pullRequestId: request.Resource.PullRequest.PullRequestId,
        repositoryId: request.Resource.PullRequest.Repository.Id,
        threadUrl: request.Resource.Comment.Links.Threads.Href,
        comment: request.Resource.Comment.Content));
    }
  }
}
