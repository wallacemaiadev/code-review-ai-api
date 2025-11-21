using NDE.Domain.Entities.CodeReviews;

namespace NDE.Application.Interfaces;

public interface IPullRequestService
{
  Task<PullRequest?> CreateAsync(PullRequest pullRequest);
  Task<int> UpdateAsync(PullRequest pullRequest);
  Task<PullRequest?> GetAsync(Guid repositoryId, int? pullRequestId = null, Guid? id = null, bool includeCodeReviews = false);
}
