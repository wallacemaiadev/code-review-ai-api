using NDE.Application.ViewModels.Request.CodeReviews;
using NDE.Application.ViewModels.Response.CodeReviews;

namespace NDE.Application.Interfaces;

public interface IAzurePullRequestService : IDisposable
{
  Task<RequestResponseViewModel?> ReviewRequestAsync(ReviewRequestViewModel diffChecker);
  Task<bool> SaveReviewAsync(SaveReviewRequestViewModel suggestions);
  Task<PreviousSuggestionsResponseViewModel?> GetPreviousSuggestions(Guid repositoryId, int pullRequestId);
  Task<bool> HandleReviewAsync(int pullRequestId, Guid repositoryId, string threadUrl, string comment);
}
