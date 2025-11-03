using System.Linq.Expressions;

using NDE.Domain.Entities.Common;

namespace NDE.Domain.Interfaces;

public interface IRepository<TEntity, TId> : IDisposable where TEntity : Entity<TId>
{
  Task Add(TEntity entity);
  Task AddRange(IEnumerable<TEntity> entity);
  Task<TEntity?> GetById(Guid id);
  Task<List<TEntity>> GetAll();
  void Update(TEntity entity);
  void UpdateRange(IEnumerable<TEntity> entity);
  void Delete(TEntity entity);
  Task<IEnumerable<TEntity?>> Search(Expression<Func<TEntity, bool>> predicate, bool tracking = false);
  Task<bool> SaveChangesAsync();
}
