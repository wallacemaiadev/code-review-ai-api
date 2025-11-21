using Microsoft.Extensions.Logging;

using NDE.Data.Context;
using NDE.Domain.Entities.CodeReviews;
using NDE.Domain.Interfaces;

namespace NDE.Data.Repositories;

public class RepositoryRepository : Repository<Repository, Guid>, IRepositoryRepository
{
  public RepositoryRepository(AppDbContext context, ILogger<Repository> logger) : base(context, logger) { }
}
