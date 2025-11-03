namespace NDE.Application.Integrations.Azure;

public interface IAzureIntegration
{
  Task<bool> CommentOnThread(string baseUrl);
}