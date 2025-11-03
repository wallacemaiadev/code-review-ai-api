
using FluentValidation;

using NDE.Domain.Entities.CodeReviews;

namespace NDE.Application.Validators;

public class AzurePullRequestValidator : AbstractValidator<AzurePullRequest>
{
  public AzurePullRequestValidator()
  {
    RuleFor(x => x.Id)
      .NotEmpty()
      .WithMessage("O Id do Pull Request é obrigatório.");
  }
}
