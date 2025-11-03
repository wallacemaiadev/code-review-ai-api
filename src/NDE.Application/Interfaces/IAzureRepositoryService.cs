using NDE.Domain.Entities.CodeReviews;

namespace NDE.Application.Interfaces;

public interface IAzureRepositoryService
{
  Task<Guid> EnsureRepositoryAsync(AzureRepository repository);
  Task<AzureRepository?> GetRepositoryByIdAsync(Guid id);
}
