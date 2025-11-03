using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace NDE.Application.ViewModels.Request.AzureWebHook;

public class BuildCompleteProjectRequestViewModel
{
  [Required(ErrorMessage = "O identificador do projeto é obrigatório.")]
  [JsonPropertyName("id")]
  public Guid Id { get; set; }

  [Required(ErrorMessage = "O nome do projeto é obrigatório.")]
  [MaxLength(200, ErrorMessage = "O nome do projeto deve ter no máximo 200 caracteres.")]
  [JsonPropertyName("name")]
  public string Name { get; set; } = string.Empty;

  [Required(ErrorMessage = "A URL do projeto é obrigatória.")]
  [Url(ErrorMessage = "A URL do projeto não é válida.")]
  [JsonPropertyName("url")]
  public string Url { get; set; } = string.Empty;
}