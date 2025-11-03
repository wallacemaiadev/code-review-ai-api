
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using NDE.Data.Context;
using NDE.Domain.Entities.CodeReviews;
using NDE.Domain.Interfaces;

namespace NDE.Data.Repository;

public class AzureProjectRepository : Repository<AzureProject, Guid>, IAzureProjectRepository
{
  public AzureProjectRepository(AppDbContext context, ILogger<AzureProject> logger) : base(context, logger) { }

  public async Task<AzureProject?> GetProjectByIdAsync(Guid projectId, bool tracking = false)
  {
    IQueryable<AzureProject> query = Db.AzureProjects;

    if (tracking)
      query = query.AsTracking();

    return await query.FirstOrDefaultAsync(
      pr => pr.Id == projectId);
  }
}
