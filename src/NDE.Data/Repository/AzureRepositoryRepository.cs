
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using NDE.Data.Context;
using NDE.Domain.Entities.CodeReviews;
using NDE.Domain.Interfaces;

namespace NDE.Data.Repository;

public class AzureRepositoryRepository : Repository<AzureRepository, Guid>, IAzureRepositoryRepository
{
  public AzureRepositoryRepository(AppDbContext context, ILogger<AzureRepository> logger) : base(context, logger) { }

  public async Task<AzureRepository?> GetRepositoryByIdAsync(Guid repositoryId, bool tracking = false)
  {
    IQueryable<AzureRepository> query = Db.AzureRepositories;

    if (tracking)
      query = query.AsTracking();

    return await query.FirstOrDefaultAsync(
      pr => pr.Id == repositoryId);
  }
}
