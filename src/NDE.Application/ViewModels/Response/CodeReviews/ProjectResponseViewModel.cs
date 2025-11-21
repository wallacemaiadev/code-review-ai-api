namespace NDE.Application.ViewModels.Response.CodeReviews
{
  public class ProjectResponseViewModel
  {
    public Guid ProjectId { get; set; } = Guid.Empty;
    public string ProjectName { get; set; } = string.Empty;
    public string ProjectUrl { get; set; } = string.Empty;
    public string CollectionUrl { get; set; } = string.Empty;
  }
}
