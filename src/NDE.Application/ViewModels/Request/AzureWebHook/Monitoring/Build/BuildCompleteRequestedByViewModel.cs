using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace NDE.Application.ViewModels.Request.AzureWebHook;

public class BuildCompleteRequestedByViewModel
{
  [Required(ErrorMessage = "O nome de exibição é obrigatório.")]
  [MaxLength(200, ErrorMessage = "O nome de exibição deve ter no máximo 200 caracteres.")]
  [JsonPropertyName("displayName")]
  public string DisplayName { get; set; } = string.Empty;

  [Required(ErrorMessage = "O identificador único do usuário é obrigatório.")]
  [EmailAddress(ErrorMessage = "O identificador único deve ser um endereço de e-mail válido.")]
  [MaxLength(200, ErrorMessage = "O identificador único deve ter no máximo 200 caracteres.")]
  [JsonPropertyName("uniqueName")]
  public string UniqueName { get; set; } = string.Empty;
}
