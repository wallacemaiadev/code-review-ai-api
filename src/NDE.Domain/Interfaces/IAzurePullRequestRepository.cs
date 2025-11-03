using NDE.Domain.Entities.CodeReviews;

namespace NDE.Domain.Interfaces;

public interface IAzurePullRequestRepository : IRepository<AzurePullRequest, int>
{
  Task<AzurePullRequest?> GetPullRequestAsync(Guid repositoryId, int pullRequestId, bool includeChildren = false, bool tracking = false);
}
