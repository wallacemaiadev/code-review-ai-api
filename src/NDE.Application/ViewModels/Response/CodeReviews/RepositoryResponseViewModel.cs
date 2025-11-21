namespace NDE.Application.ViewModels.Response.CodeReviews
{
  public class RepositoryResponseViewModel
  {
    public Guid RepositoryId { get; set; } = Guid.Empty;
    public string RepositoryName { get; set; } = string.Empty;
    public string RepositoryUrl { get; set; } = string.Empty;
  }
}
