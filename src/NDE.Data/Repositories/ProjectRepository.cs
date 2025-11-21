
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using NDE.Data.Context;
using NDE.Domain.Entities.CodeReviews;
using NDE.Domain.Interfaces;

namespace NDE.Data.Repositories;

public class ProjectRepository : Repository<Project, Guid>, IProjectRepository
{
  public ProjectRepository(AppDbContext context, ILogger<Project> logger) : base(context, logger) { }

  public async Task<Project?> GetByIdAsync(Guid projectId, bool tracking = false)
  {
    IQueryable<Project> query = Db.Projects;

    if (tracking)
      query = query.AsTracking();

    return await query.FirstOrDefaultAsync(
      pr => pr.Id == projectId);
  }
}
