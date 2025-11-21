using System.Text.Json;

namespace NDE.Domain.Entities.Common
{
  public class JobStoreDeadLetter
  {
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public int Attempts { get; set; }
    public string Payload { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;

    public JobStoreDeadLetter(Guid id, object request, int attempts, string error = "")
    {
      Id = id;
      CreatedAt = DateTime.UtcNow;
      Attempts = attempts;
      Payload = JsonSerializer.Serialize(request);
      Error = error;
    }

    public JobStoreDeadLetter() { }
  }
}
