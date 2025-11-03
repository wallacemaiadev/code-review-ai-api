
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using NDE.Data.Context;
using NDE.Domain.Entities.CodeReviews;
using NDE.Domain.Interfaces;

namespace NDE.Data.Repository;

public class CodeReviewRepository : Repository<CodeReview, Guid>, ICodeReviewRepository
{
  public CodeReviewRepository(AppDbContext context, ILogger<CodeReview> logger) : base(context, logger) { }

  public async Task<CodeReview?> GetCodeReviewAsync(
    int? pullRequestId = null,
    string? filePath = null,
    int? verdictId = null,
    string? fingerprint = null,
    bool closed = false,
    bool tracking = false)
  {
    IQueryable<CodeReview> query = Db.CodeReviews;

    if (tracking)
      query = query.AsTracking();

    query = query.Where(cr => cr.Closed == closed);

    if (pullRequestId != null)
      query = query.Where(cr => cr.PullRequestId == pullRequestId);

    if (filePath != null)
      query = query.Where(cr => cr.FilePath == filePath);

    if (verdictId != null)
      query = query.Where(cr => cr.VerdictId == verdictId);

    if (fingerprint != null)
      query = query.Where(cr => cr.Fingerprint == fingerprint);

    return await query.FirstOrDefaultAsync();
  }

  public async Task<CodeReview?> GetSummaryReview(int verdictId, Guid? repositoryId = null, int? pullRequestId = null, bool tracking = false)
  {
    IQueryable<CodeReview> query = Db.CodeReviews;

    if (tracking)
      query = query.AsTracking();

    if (repositoryId != null)
      query = query.Include(cr => cr.AzurePullRequest).Where(cr => cr.AzurePullRequest.RepositoryId == repositoryId);

    if (pullRequestId != null)
      query = query.Where(cr => cr.PullRequestId == pullRequestId);

    return await query
      .Where(cr => cr.VerdictId == verdictId)
      .OrderByDescending(cr => cr.CreatedAt)
      .FirstOrDefaultAsync();
  }

  public async Task<CodeReview?> GetSummaryReviewApproved(int pullRequestId, Guid repositoryId, bool tracking = false)
  {
    var approvedByPr = await GetSummaryReview(
        verdictId: Verdict.Approved.Id,
        pullRequestId: pullRequestId,
        tracking: tracking);

    if (approvedByPr != null)
      return approvedByPr;

    var approvedByRepos = await GetSummaryReview(
        verdictId: Verdict.Approved.Id,
        repositoryId: repositoryId,
        tracking: tracking);

    return approvedByRepos;
  }

  public async Task<CodeReview?> GetSummaryReviewReproved(int pullRequestId, Guid repositoryId, bool tracking = false)
  {
    var reprovedByPr = await GetSummaryReview(
        verdictId: Verdict.Reproved.Id,
        pullRequestId: pullRequestId,
        tracking: tracking);

    if (reprovedByPr != null)
      return reprovedByPr;

    var reprovedByRepos = await GetSummaryReview(
        verdictId: Verdict.Reproved.Id,
        repositoryId: repositoryId,
        tracking: tracking);

    return reprovedByRepos;
  }
}