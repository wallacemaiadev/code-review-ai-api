using System.ComponentModel.DataAnnotations;

namespace NDE.Application.ViewModels.Request.CodeReviews;

public class UpdateCodeReviewRequestViewModel
{
  [Required(ErrorMessage = "O Id do Code Review é obrigatório.")]
  public Guid Id { get; set; }

  [Required(ErrorMessage = "O Feedback é obrigatório.")]
  [MinLength(5, ErrorMessage = "O Feedback deve conter ao menos 5 caracteres.")]
  public string Feedback { get; set; } = string.Empty;

  [Required(ErrorMessage = "O VerdictId é obrigatório.")]
  [Range(-2, 3, ErrorMessage = "O VerdictId informado não é válido.")]
  public int VerdictId { get; set; }
}
