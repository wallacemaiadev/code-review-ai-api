using System.ComponentModel.DataAnnotations;

namespace NDE.Application.ViewModels.Request.CodeReviews;

public class ModificationRequestViewModel
{
  [Required(ErrorMessage = "O bloco de código é obrigatório.")]
  public string CodeBlock { get; set; } = string.Empty;
}
