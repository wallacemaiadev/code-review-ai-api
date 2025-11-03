using NDE.Domain.Entities.CodeReviews;

namespace NDE.Domain.Interfaces;

public interface ICodeReviewRepository : IRepository<CodeReview, Guid>
{
  Task<CodeReview?> GetCodeReviewAsync(
    int? pullRequestId = null,
    string? filePath = null,
    int? verdictId = null,
    string? fingerprint = null,
    bool closed = false,
    bool tracking = false);

  Task<CodeReview?> GetSummaryReview(int verdictId, Guid? repositoryId = null, int? pullRequestId = null, bool tracking = false);

  Task<CodeReview?> GetSummaryReviewApproved(int pullRequestId, Guid repositoryId, bool tracking = false);

  Task<CodeReview?> GetSummaryReviewReproved(int pullRequestId, Guid repositoryId, bool tracking = false);
}