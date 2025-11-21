using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using NDE.Data.Context;
using NDE.Domain.Entities.Common;
using NDE.Domain.Interfaces;

namespace NDE.Data.Repositories;

public abstract class Repository<TEntity, TId> : IRepository<TEntity, TId> where TEntity : Entity<TId>
{
  protected readonly AppDbContext Db;
  protected readonly DbSet<TEntity> DbSet;
  protected readonly ILogger<TEntity> _logger;
  protected Repository(AppDbContext db, ILogger<TEntity> logger)
  {
    Db = db;
    DbSet = db.Set<TEntity>();
    _logger = logger;
  }

  public async Task<IEnumerable<TEntity?>> Search(Expression<Func<TEntity, bool>> predicate, bool tracking = false)
  {
    IQueryable<TEntity> query = DbSet;

    if (tracking)
      query = query.AsTracking();

    return await query.Where(predicate).ToListAsync();
  }

  public virtual async Task<TEntity?> GetById(TId id)
  {
    return await DbSet.FindAsync(id);
  }

  public async Task<List<TEntity>> GetAll()
  {
    return await DbSet.ToListAsync();
  }

  public async Task Add(TEntity entity)
  {
    await DbSet.AddAsync(entity);
  }

  public async Task AddRange(IEnumerable<TEntity> entity)
  {
    await DbSet.AddRangeAsync(entity);
  }

  public void Update(TEntity entity)
  {
    DbSet.Update(entity);
  }
  public void UpdateRange(IEnumerable<TEntity> entity)
  {
    DbSet.UpdateRange(entity);
  }
  public void Delete(TEntity entity)
  {
    DbSet.Remove(entity);
  }

  public async Task<bool> SaveChangesAsync()
  {
    using var transaction = await Db.Database.BeginTransactionAsync();
    try
    {
      var status = await Db.SaveChangesAsync();
      if (status == 0)
      {
        _logger.LogInformation("[INFO] Nenhuma linha foi alterada no banco de dados.");
      }

      await transaction.CommitAsync();
      return true;
    }
    catch (Exception ex)
    {
      await transaction.RollbackAsync();

      _logger.LogError(ex, "[ERROR] Falha ao persistir {Entity} no banco de dados. Mensagem: {Message}",
          typeof(TEntity).Name, ex.Message);

      return false;
    }
  }
}