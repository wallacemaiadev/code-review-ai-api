namespace NDE.Application.ViewModels.Response.CodeReviews;

public class RequestResponseViewModel
{
  public string Fingerprint { get; set; } = string.Empty;
  public string Suggestion { get; set; } = string.Empty;
  public int TokensConsumed { get; set; }
  public bool FromCache { get; set; }
}
