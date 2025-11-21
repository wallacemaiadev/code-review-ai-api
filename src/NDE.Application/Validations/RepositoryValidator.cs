using FluentValidation;

using NDE.Domain.Entities.CodeReviews;

namespace NDE.Application.Validators;

public class RepositoryValidator : AbstractValidator<Repository>
{
  public RepositoryValidator()
  {
    RuleFor(x => x.Id)
      .NotEmpty()
      .WithMessage("O Id do repositório é obrigatório.");

    RuleFor(x => x.ProjectId)
      .NotEmpty()
      .WithMessage("O Id do projeto é obrigatório.");

    RuleFor(x => x.Name)
      .NotEmpty()
      .WithMessage("O nome é obrigatório.")
      .MaximumLength(200)
      .WithMessage("O nome deve ter no máximo 200 caracteres.");

    RuleFor(x => x.Url)
      .NotEmpty()
      .WithMessage("A URL é obrigatória.")
      .MaximumLength(500)
      .WithMessage("A URL deve ter no máximo 500 caracteres.")
      .Must(BeAValidUrl)
      .WithMessage("A URL não é válida.");
  }

  private static bool BeAValidUrl(string url) =>
    Uri.TryCreate(url, UriKind.Absolute, out _);
}
