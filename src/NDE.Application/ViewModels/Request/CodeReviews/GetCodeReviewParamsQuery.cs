using Microsoft.AspNetCore.Mvc;

namespace NDE.Application.ViewModels.Request.CodeReviews;

public class GetCodeReviewParamsQuery
{
  [FromQuery(Name = "reposId")]
  public Guid ReposId { get; set; }

  [FromQuery(Name = "projectId")]
  public Guid ProjectId { get; set; }

  [FromQuery(Name = "prId")]
  public Guid PrId { get; set; }

  [FromQuery(Name = "token")]
  public string AuthToken { get; set; } = string.Empty;
}