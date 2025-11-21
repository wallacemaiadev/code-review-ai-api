using NDE.Domain.Entities.CodeReviews;

namespace NDE.Domain.Interfaces;

public interface ICodeReviewRepository : IRepository<CodeReview, Guid>
{
  Task<CodeReview?> GetCodeReviewAsync(
    Guid? id = null,
    Guid? pullRequestId = null,
    string? filePath = null,
    int? verdictId = null,
    bool? closed = null,
    bool tracking = false);
  Task<List<CodeReview>?> GetListCodeReviewAsync(int? verdictId = null, Guid? repositoryId = null, Guid? pullRequestId = null, bool tracking = false);
}