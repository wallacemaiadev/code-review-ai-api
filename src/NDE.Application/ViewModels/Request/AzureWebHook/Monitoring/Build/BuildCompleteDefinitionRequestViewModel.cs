using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace NDE.Application.ViewModels.Request.AzureWebHook;

public class BuildCompleteDefinitionRequestViewModel
{
  [Required(ErrorMessage = "O tipo da definição é obrigatório.")]
  [MaxLength(100, ErrorMessage = "O tipo da definição deve ter no máximo 100 caracteres.")]
  [JsonPropertyName("type")]
  public string Type { get; set; } = string.Empty;

  [Required(ErrorMessage = "As informações do projeto são obrigatórias.")]
  [JsonPropertyName("project")]
  public BuildCompleteProjectRequestViewModel Project { get; set; } = new();
}
