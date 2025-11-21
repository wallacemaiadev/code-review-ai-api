
using FluentValidation;

using NDE.Domain.Entities.CodeReviews;

namespace NDE.Application.Validators;

public class PullRequestValidator : AbstractValidator<PullRequest>
{
  public PullRequestValidator()
  {
    RuleFor(x => x.Id)
      .NotEmpty()
      .WithMessage("O Id do Pull Request é obrigatório.");

    RuleFor(x => x.RepositoryId)
      .NotEmpty()
      .WithMessage("O Id do Repositório é obrigatório.");

    RuleFor(x => x.PullRequestId)
      .GreaterThan(0)
      .WithMessage("O PullRequestId deve ser maior que zero.");

    RuleFor(x => x.Title)
      .NotEmpty()
      .WithMessage("O título é obrigatório.")
      .MaximumLength(300)
      .WithMessage("O título deve ter no máximo 300 caracteres.");
  }
}
