namespace NDE.Application.ViewModels.Response.CodeReviews;

public class CodeReviewResponseViewModel
{
  public Guid Id { get; set; }
  public string Language { get; set; } = string.Empty;
  public string FilePath { get; set; } = string.Empty;
  public string FileLink { get; set; } = string.Empty;
  public string Suggestion { get; set; } = string.Empty;
  public string Feedback { get; set; } = string.Empty;
  public int VerdictId { get; set; }
  public int TokensConsumed { get; set; }
  public List<ModificationResponseViewModel> CodeModifications { get; set; } = new();
}
