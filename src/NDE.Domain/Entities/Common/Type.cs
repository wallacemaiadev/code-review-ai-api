using System.ComponentModel.DataAnnotations;

namespace NDE.Domain.Entities.Common
{
  public class Type<T> where T : struct
  {
    [Key]
    public T Id { get; set; }
    public string Description { get; set; } = string.Empty;

    public override string ToString() => Description;
  }
}