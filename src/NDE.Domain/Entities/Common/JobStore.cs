namespace NDE.Domain.Entities.Common
{
  public class JobStore
  {
    public Guid Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Result { get; set; }
    public int Attempts { get; set; }
  }
}
