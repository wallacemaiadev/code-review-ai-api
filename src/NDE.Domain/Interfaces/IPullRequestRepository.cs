using NDE.Domain.Entities.CodeReviews;

namespace NDE.Domain.Interfaces;

public interface IPullRequestRepository : IRepository<PullRequest, Guid>
{
  Task<PullRequest?> GetPullRequestAsync(Guid repositoryId, int? pullRequestId = null, Guid? id = null, bool includeChildren = false, bool tracking = false);
}
