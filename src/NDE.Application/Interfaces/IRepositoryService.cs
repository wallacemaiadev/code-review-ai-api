using NDE.Domain.Entities.CodeReviews;

namespace NDE.Application.Interfaces;

public interface IRepositoryService
{
  Task<Guid> CreateAsync(Repository repository);
  Task<Repository?> GetByIdAsync(Guid id);
}
