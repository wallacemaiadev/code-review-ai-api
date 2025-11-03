using System.ComponentModel.DataAnnotations;

namespace NDE.Application.ViewModels.Request.CodeReviews;

public class ReviewRequestViewModel
{
  [Required(ErrorMessage = "O PullRequestId é obrigatório.")]
  public int PullRequestId { get; set; }

  [Required(ErrorMessage = "O RepositoryId é obrigatório.")]
  public Guid RepositoryId { get; set; }

  [Required(ErrorMessage = "A linguagem é obrigatória.")]
  [StringLength(50, ErrorMessage = "A linguagem deve ter no máximo 50 caracteres.")]
  public string Language { get; set; } = string.Empty;

  [Required(ErrorMessage = "O caminho do arquivo é obrigatório.")]
  [StringLength(500, ErrorMessage = "O caminho do arquivo deve ter no máximo 500 caracteres.")]
  public string FilePath { get; set; } = string.Empty;

  [Required(ErrorMessage = "O diff é obrigatório.")]
  [MinLength(5, ErrorMessage = "O diff deve ter pelo menos 5 caracteres.")]
  public string Diff { get; set; } = string.Empty;
}

public class PreviousSuggestionsRequestViewModel
{
  public Guid RepositoryId { get; set; }
  public int PullRequestId { get; set; }
}
