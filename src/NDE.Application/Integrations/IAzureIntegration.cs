using NDE.Application.Integrations.Models;
using NDE.Application.ViewModels.Response.CodeReviews;

namespace NDE.Application.Integrations.Azure;

public interface IAzureIntegration
{
  Task<int> AddCodeReviewToPR(
    string collectionUrl,
    string projectName,
    Guid repositoryId,
    string portalUrl,
    string repositoryUrl,
    string token,
    int pullRequestId,
    List<LLMOutputResponseViewModel> reviews);
  Task CloseExistingCommentsAsync(string collectionUrl, string projectName, Guid repositoryId, string token, int pullRequestId);
  Task<AzurePullRequestInfoResponse> GetPullRequestInfoAsync(string collectionUrl, string projectName, Guid repositoryId, string token, int pullRequestId);
}