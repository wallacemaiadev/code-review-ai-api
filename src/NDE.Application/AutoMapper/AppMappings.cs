using Mapster;

using NDE.Application.ViewModels.Request.CodeReviews;
using NDE.Application.ViewModels.Response.CodeReviews;
using NDE.Domain.Entities.CodeReviews;

namespace NDE.Application.AutoMapper;

public class MappingRegistration : IRegister
{
  public void Register(TypeAdapterConfig config)
  {
    config.NewConfig<ProjectRequestViewModel, Project>()
      .ConstructUsing(src => new Project(
        src.Id,
        src.Name,
        src.Description,
        src.Url,
        src.CollectionUrl));

    config.NewConfig<RepositoryRequestViewModel, Repository>()
      .ConstructUsing(src => new Repository(
        src.Id,
        src.ProjectId,
        src.Name,
        src.Description,
        src.Url));

    config.NewConfig<CodeReview, CodeReviewResponseViewModel>();

    config.NewConfig<Modification, ModificationResponseViewModel>();
  }
}
