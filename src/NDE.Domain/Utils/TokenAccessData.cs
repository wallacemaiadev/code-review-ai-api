namespace NDE.Domain.Utils
{
  public class TokenAccessData
  {
    public Dictionary<string, string> ResourceParams { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
  }
}
