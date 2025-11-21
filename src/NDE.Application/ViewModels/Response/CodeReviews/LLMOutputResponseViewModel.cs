namespace NDE.Application.ViewModels.Response.CodeReviews;

public class LLMOutputResponseViewModel
{
  public string FilePath { get; set; } = string.Empty;
  public string Suggestion { get; set; } = string.Empty;
  public string Verdict { get; set; } = string.Empty;
  public string FileLink { get; set; } = string.Empty;
}
