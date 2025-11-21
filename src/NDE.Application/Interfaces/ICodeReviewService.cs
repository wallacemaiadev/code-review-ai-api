using NDE.Application.ViewModels.Request.CodeReviews;
using NDE.Application.ViewModels.Response.CodeReviews;

namespace NDE.Application.Interfaces;

public interface ICodeReviewService
{
  Task<bool> ReviewAsync(StartReviewRequestViewModel request);
  Task<PullRequestResponseViewModel?> GetAllCodeReviewAsync(Guid repositoryId, Guid projectId, Guid pullRequestId);
  Task<bool> UpdateCodeReviewAsync(UpdateCodeReviewRequestViewModel request);
}
