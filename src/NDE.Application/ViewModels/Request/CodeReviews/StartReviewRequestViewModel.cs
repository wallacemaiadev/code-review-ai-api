using System.ComponentModel.DataAnnotations;

namespace NDE.Application.ViewModels.Request.CodeReviews;

public class StartReviewRequestViewModel
{
  [Required(ErrorMessage = "O PullRequestId é obrigatório.")]
  public int PullRequestId { get; set; }

  [Required(ErrorMessage = "O título do Pull Request é obrigatório.")]
  [MaxLength(500, ErrorMessage = "O título deve ter no máximo 500 caracteres.")]
  public string Title { get; set; } = string.Empty;

  [Required(ErrorMessage = "A descrição do Pull Request é obrigatória.")]
  public string Description { get; set; } = string.Empty;

  [Required(ErrorMessage = "O AuthToken é obrigatório.")]
  public string AuthToken { get; set; } = string.Empty;

  public string SystemPrompt { get; set; } = string.Empty;

  [Required(ErrorMessage = "O ProjectId é obrigatório.")]
  public Guid ProjectId { get; set; }

  [Required(ErrorMessage = "O RepositoryId é obrigatório.")]
  public Guid RepositoryId { get; set; }

  public List<GitDiffEntryRequestViewModel> Entries { get; set; } = new();
}
