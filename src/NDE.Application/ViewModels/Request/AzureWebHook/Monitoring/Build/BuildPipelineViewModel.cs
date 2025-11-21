using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace NDE.Application.ViewModels.Request.AzureWebHook;

public class BuildCompleteRequestViewModel
{
  [Required(ErrorMessage = "O tipo do evento é obrigatório.")]
  [MaxLength(100, ErrorMessage = "O tipo do evento deve ter no máximo 100 caracteres.")]
  [JsonPropertyName("eventType")]
  public string EventType { get; set; } = string.Empty;

  [Required(ErrorMessage = "Os dados do recurso do evento são obrigatórios.")]
  [JsonPropertyName("resource")]
  public BuildCompleteResourceViewModel Resource { get; set; } = default!;
}