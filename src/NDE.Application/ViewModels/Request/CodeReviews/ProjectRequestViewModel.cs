using System.ComponentModel.DataAnnotations;

namespace NDE.Application.ViewModels.Request.CodeReviews;

public class ProjectRequestViewModel
{
  [Required(ErrorMessage = "O ProjectId é obrigatório.")]
  public Guid ProjectId { get; set; }

  [Required(ErrorMessage = "O nome do projeto é obrigatório.")]
  [MaxLength(200, ErrorMessage = "O nome do projeto deve ter no máximo 200 caracteres.")]
  public string ProjectName { get; set; } = string.Empty;

  [Required(ErrorMessage = "A URI do projeto é obrigatória.")]
  [MaxLength(500, ErrorMessage = "A URI deve ter no máximo 500 caracteres.")]
  [Url(ErrorMessage = "A URI do projeto não é válida.")]
  public string ProjectUrl { get; set; } = string.Empty;
}
