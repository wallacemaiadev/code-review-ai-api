
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using NDE.Data.Context;
using NDE.Domain.Entities.CodeReviews;
using NDE.Domain.Interfaces;

namespace NDE.Data.Repository;

public class AzurePullRequestRepository : Repository<AzurePullRequest, int>, IAzurePullRequestRepository
{
  public AzurePullRequestRepository(AppDbContext context, ILogger<AzurePullRequest> logger) : base(context, logger) { }

  public async Task<AzurePullRequest?> GetPullRequestAsync(Guid repositoryId, int pullRequestId, bool includeChildren = false, bool tracking = false)
  {
    IQueryable<AzurePullRequest> query = Db.PullRequests;

    if (includeChildren)
      query = query
        .Include(pr => pr.CodeReviews)
          .AsSplitQuery();

    if (tracking)
      query = query.AsTracking();

    return await query.FirstOrDefaultAsync(
      pr => pr.Id == pullRequestId && pr.RepositoryId == repositoryId);
  }
}
