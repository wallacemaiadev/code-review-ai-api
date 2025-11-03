using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace NDE.Application.ViewModels.Request.AzureWebHook;

public class BuildCompleteResourceViewModel
{
  [Required(ErrorMessage = "O identificador do build é obrigatório.")]
  [JsonPropertyName("id")]
  public int Id { get; set; }

  [Required(ErrorMessage = "O número do build é obrigatório.")]
  [MaxLength(100, ErrorMessage = "O número do build deve ter no máximo 100 caracteres.")]
  [JsonPropertyName("buildNumber")]
  public string BuildNumber { get; set; } = string.Empty;

  [Required(ErrorMessage = "O status do build é obrigatório.")]
  [MaxLength(50, ErrorMessage = "O status deve ter no máximo 50 caracteres.")]
  [JsonPropertyName("status")]
  public string Status { get; set; } = string.Empty;

  [Required(ErrorMessage = "O resultado do build é obrigatório.")]
  [MaxLength(50, ErrorMessage = "O resultado deve ter no máximo 50 caracteres.")]
  [JsonPropertyName("result")]
  public string Result { get; set; } = string.Empty;

  [Required(ErrorMessage = "A data/hora de enfileiramento é obrigatória.")]
  [JsonPropertyName("queueTime")]
  public DateTime QueueTime { get; set; }

  [Required(ErrorMessage = "A data/hora de início é obrigatória.")]
  [JsonPropertyName("startTime")]
  public DateTime StartTime { get; set; }

  [Required(ErrorMessage = "A data/hora de finalização é obrigatória.")]
  [JsonPropertyName("finishTime")]
  public DateTime FinishTime { get; set; }

  [Required(ErrorMessage = "A URL do build é obrigatória.")]
  [Url(ErrorMessage = "A URL do build não é válida.")]
  [JsonPropertyName("url")]
  public string Url { get; set; } = string.Empty;

  [Required(ErrorMessage = "O branch de origem é obrigatório.")]
  [MaxLength(200, ErrorMessage = "O branch de origem deve ter no máximo 200 caracteres.")]
  [JsonPropertyName("sourceBranch")]
  public string SourceBranch { get; set; } = string.Empty;

  [Required(ErrorMessage = "As informações de quem requisitou o build são obrigatórias.")]
  [JsonPropertyName("requestedBy")]
  public BuildCompleteRequestedByViewModel RequestedBy { get; set; } = default!;

  [Required(ErrorMessage = "As informações do repositório são obrigatórias.")]
  [JsonPropertyName("repository")]
  public BuildCompleteRepositoryViewModel Repository { get; set; } = default!;

  [Required(ErrorMessage = "As informações da definição são obrigatórias.")]
  [JsonPropertyName("definition")]
  public BuildCompleteDefinitionRequestViewModel Definition { get; set; } = default!;
}
