using System.ComponentModel.DataAnnotations;

namespace NDE.Application.ViewModels.Request.CodeReviews;

public class SuggestionRequestViewModel
{
  [Required(ErrorMessage = "O fingerprint do diff é obrigatório.")]
  public string Fingerprint { get; set; } = string.Empty;

  [Required(ErrorMessage = "A Sugestão da AI é obrigatório.")]
  public string Suggestion { get; set; } = string.Empty;

  [Required(ErrorMessage = "A quantidade de tokens consunidos é obrigatório.")]
  public int TokensConsumed { get; set; }
}
