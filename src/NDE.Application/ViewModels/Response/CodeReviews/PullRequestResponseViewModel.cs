namespace NDE.Application.ViewModels.Response.CodeReviews;

public class PullRequestResponseViewModel
{
  public Guid Id { get; set; }
  public int PullRequestId { get; set; }
  public string Title { get; set; } = string.Empty;
  public int TokensConsumed { get; set; }
  public List<CodeReviewResponseViewModel> CodeReviews { get; set; } = new();
}
