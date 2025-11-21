using NDE.Domain.Entities.CodeReviews;

namespace NDE.Application.Interfaces;

public interface IProjectService
{
  Task<Guid> CreateAsync(Project project);
  Task<Project?> GetByIdAsync(Guid id);
}
