namespace NDE.VectorStore.Models
{
  public class QdrantPointResponse
  {
    public QdrantPoint? Result { get; set; }
    public string? Status { get; set; }
    public double Time { get; set; }
  }

  public class QdrantPoint
  {
    public string Id { get; set; } = string.Empty;
    public object? Payload { get; set; }
    public float[]? Vector { get; set; }
  }
}