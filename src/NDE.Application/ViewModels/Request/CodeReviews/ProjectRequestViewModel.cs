using System.ComponentModel.DataAnnotations;

namespace NDE.Application.ViewModels.Request.CodeReviews;

public class ProjectRequestViewModel
{
  [Required(ErrorMessage = "O Id é obrigatório.")]
  public Guid Id { get; set; }

  [Required(ErrorMessage = "O nome do projeto é obrigatório.")]
  [MaxLength(200, ErrorMessage = "O nome do projeto deve ter no máximo 200 caracteres.")]
  public string Name { get; set; } = string.Empty;

  public string Description { get; set; } = string.Empty;

  [Required(ErrorMessage = "A URL do projeto é obrigatória.")]
  [MaxLength(500, ErrorMessage = "A URL do projeto deve ter no máximo 500 caracteres.")]
  public string Url { get; set; } = string.Empty;

  [Required(ErrorMessage = "A URL da collection é obrigatória.")]
  [MaxLength(500, ErrorMessage = "A URL da collection deve ter no máximo 500 caracteres.")]
  public string CollectionUrl { get; set; } = string.Empty;
}
