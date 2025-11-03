using NDE.Domain.Entities.CodeReviews;

namespace NDE.Domain.Interfaces;

public interface IAzureProjectRepository : IRepository<AzureProject, Guid>
{
  Task<AzureProject?> GetProjectByIdAsync(Guid projectId, bool tracking = false);
}
