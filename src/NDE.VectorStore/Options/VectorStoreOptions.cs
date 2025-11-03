namespace NDE.VectorStore.Options;

public class VectorStoreOptions
{
  public string Endpoint { get; set; } = string.Empty;
  public int Port { get; set; }
  public string ApiKey { get; set; } = string.Empty;
  public string CollectionCodeMemory { get; set; } = string.Empty;
  public string CollectionRepoMeta { get; set; } = string.Empty;
  public string CollectionExamples { get; set; } = string.Empty;
  public int Dimensions { get; set; } = 768;
  public string Distance { get; set; } = "Cosine";
}
