using NDE.Domain.Entities.CodeReviews;

namespace NDE.Domain.Interfaces;

public interface IAzureRepositoryRepository : IRepository<AzureRepository, Guid>
{
  Task<AzureRepository?> GetRepositoryByIdAsync(Guid repositoryId, bool tracking = false);
}
