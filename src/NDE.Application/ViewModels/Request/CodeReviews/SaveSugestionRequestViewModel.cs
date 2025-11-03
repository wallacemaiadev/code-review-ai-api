using System.ComponentModel.DataAnnotations;

namespace NDE.Application.ViewModels.Request.CodeReviews;

public class SaveReviewRequestViewModel
{
  [Required(ErrorMessage = "O PullRequestId é obrigatório.")]
  public int PullRequestId { get; set; }

  [Required(ErrorMessage = "O RepositoryId é obrigatório.")]
  public Guid RepositoryId { get; set; }

  public List<SuggestionRequestViewModel> Suggestions { get; set; } = new();
}
