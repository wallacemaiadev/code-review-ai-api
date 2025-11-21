
namespace NDE.Domain.Entities.Common;

public abstract class Entity<T>
{
  public T? Id { get; protected set; }

  protected Entity() { }

  protected Entity(T id)
  {
    Id = id;
  }
}