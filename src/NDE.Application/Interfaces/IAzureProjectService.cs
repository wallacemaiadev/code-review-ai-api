using NDE.Domain.Entities.CodeReviews;

namespace NDE.Application.Interfaces;

public interface IAzureProjectService
{
  Task<Guid> EnsureProjectAsync(AzureProject project);
  Task<AzureProject?> GetProjectByIdAsync(Guid id);
}
