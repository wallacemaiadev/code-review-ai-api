using AutoMapper;

using NDE.Application.ViewModels.Request.CodeReviews;
using NDE.Domain.Entities.CodeReviews;

namespace NDE.Application.AutoMapper;

public class CodeReviewProfile : Profile
{
  public CodeReviewProfile()
  {
    CreateMap<ProjectRequestViewModel, AzureProject>()
      .ConstructUsing(src => new AzureProject(src.ProjectId, src.ProjectName, src.ProjectUrl));

    CreateMap<RepositoryRequestViewModel, AzureRepository>()
      .ConstructUsing(src => new AzureRepository(src.RepositoryId, src.ProjectId, src.RepositoryName, src.RepositoryUrl));
  }
}
