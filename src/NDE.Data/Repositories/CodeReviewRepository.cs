
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using NDE.Data.Context;
using NDE.Domain.Entities.CodeReviews;
using NDE.Domain.Interfaces;

namespace NDE.Data.Repositories;

public class CodeReviewRepository : Repository<CodeReview, Guid>, ICodeReviewRepository
{
  public CodeReviewRepository(AppDbContext context, ILogger<CodeReview> logger) : base(context, logger) { }

  public async Task<CodeReview?> GetCodeReviewAsync(
    Guid? id = null,
    Guid? pullRequestId = null,
    string? filePath = null,
    int? verdictId = null,
    bool? closed = null,
    bool tracking = false)
  {
    IQueryable<CodeReview> query = Db.CodeReviews;

    if (tracking)
      query = query.AsTracking();

    if (id != null)
      query = query.Where(cr => cr.Id == id);

    if (closed != null)
      query = query.Where(cr => cr.Closed == closed);

    if (pullRequestId != null)
      query = query.Where(cr => cr.PullRequestId == pullRequestId);

    if (filePath != null)
      query = query.Where(cr => cr.FilePath == filePath);

    if (verdictId != null)
      query = query.Where(cr => cr.VerdictId == verdictId);


    return await query.FirstOrDefaultAsync();
  }

  public async Task<List<CodeReview>?> GetListCodeReviewAsync(int? verdictId = null, Guid? repositoryId = null, Guid? pullRequestId = null, bool tracking = false)
  {
    IQueryable<CodeReview> query = Db.CodeReviews.Include(cr => cr.Modifications);

    if (tracking)
      query = query.AsTracking();

    if (repositoryId != null)
      query = query.Include(cr => cr.PullRequest).Where(cr => cr.PullRequest.RepositoryId == repositoryId);

    if (pullRequestId != null)
      query = query.Where(cr => cr.PullRequestId == pullRequestId);

    if (verdictId != null)
      query = query.Where(cr => cr.VerdictId == verdictId);

    return await query
      .OrderByDescending(cr => cr.CreatedAt)
      .ToListAsync();
  }
}