using System.ComponentModel.DataAnnotations;

namespace NDE.Application.ViewModels.Request.CodeReviews;

public class RepositoryRequestViewModel
{
  [Required(ErrorMessage = "O RepositoryId é obrigatório.")]
  public Guid Id { get; set; }

  [Required(ErrorMessage = "O ProjectId é obrigatório.")]
  public Guid ProjectId { get; set; }

  [Required(ErrorMessage = "O nome do repositório é obrigatório.")]
  [MaxLength(200, ErrorMessage = "O nome do repositório deve ter no máximo 200 caracteres.")]
  public string Name { get; set; } = string.Empty;

  public string Description { get; set; } = string.Empty;

  
  [Required(ErrorMessage = "A URL do repositório é obrigatória.")]
  [MaxLength(500, ErrorMessage = "A URL do repositório deve ter no máximo 500 caracteres.")]
  public string Url { get; set; } = string.Empty;
}
