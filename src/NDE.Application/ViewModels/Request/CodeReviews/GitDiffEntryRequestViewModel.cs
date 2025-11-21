using System.ComponentModel.DataAnnotations;

namespace NDE.Application.ViewModels.Request.CodeReviews;

public class GitDiffEntryRequestViewModel
{
  [Required(ErrorMessage = "A linguagem é obrigatória.")]
  [StringLength(50, ErrorMessage = "A linguagem deve ter no máximo 50 caracteres.")]
  public string Language { get; set; } = string.Empty;

  [Required(ErrorMessage = "O caminho do arquivo é obrigatório.")]
  [StringLength(500, ErrorMessage = "O caminho do arquivo deve ter no máximo 500 caracteres.")]
  public string FilePath { get; set; } = string.Empty;

  [Required(ErrorMessage = "O diff é obrigatório.")]
  [MinLength(5, ErrorMessage = "O diff deve conter pelo menos 5 caracteres.")]
  public string Diff { get; set; } = string.Empty;

  [Required(ErrorMessage = "O prompt é obrigatório.")]
  [MinLength(5, ErrorMessage = "O prompt deve conter pelo menos 5 caracteres.")]
  public string Prompt { get; set; } = string.Empty;

  public List<ModificationRequestViewModel> Modifications { get; set; } = new();
}
