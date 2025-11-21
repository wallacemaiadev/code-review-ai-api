
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using NDE.Data.Context;
using NDE.Domain.Entities.CodeReviews;
using NDE.Domain.Interfaces;

namespace NDE.Data.Repositories;

public class PullRequestRepository : Repository<PullRequest, Guid>, IPullRequestRepository
{
  public PullRequestRepository(AppDbContext context, ILogger<PullRequest> logger) : base(context, logger) { }

  public async Task<PullRequest?> GetPullRequestAsync(Guid repositoryId, int? pullRequestId = null, Guid? id = null, bool includeChildren = false, bool tracking = false)
  {
    IQueryable<PullRequest> query = Db.PullRequests;

    if (includeChildren)
      query = query
        .Include(pr => pr.CodeReviews)
          .AsSplitQuery();

    if (id != null)
      query = query.Where(pr => pr.Id == id);

    if (pullRequestId != null)
      query = query.Where(pr => pr.PullRequestId == pullRequestId);

    if (tracking)
      query = query.AsTracking();
  
    return await query.FirstOrDefaultAsync(
      pr => pr.RepositoryId == repositoryId);
  }
}
