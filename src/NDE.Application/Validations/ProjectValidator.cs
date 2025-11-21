
using FluentValidation;

using NDE.Domain.Entities.CodeReviews;

namespace NDE.Application.Validators;

public class ProjectValidator : AbstractValidator<Project>
{
  public ProjectValidator()
  {
    RuleFor(x => x.Id)
      .NotEmpty()
      .WithMessage("O Id do projeto é obrigatório.");

    RuleFor(x => x.Name)
      .NotEmpty()
      .WithMessage("O nome do projeto é obrigatório.")
      .MaximumLength(200)
      .WithMessage("O nome do projeto deve ter no máximo 200 caracteres.");

    RuleFor(x => x.Url)
      .NotEmpty()
      .WithMessage("A URI do projeto é obrigatória.")
      .MaximumLength(500)
      .WithMessage("A URI deve ter no máximo 500 caracteres.")
      .Must(BeAValidUrl)
      .WithMessage("A URI do projeto não é válida.");

    RuleFor(x => x.CollectionUrl)
      .NotEmpty()
      .WithMessage("A URL da Collection é obrigatória.")
      .MaximumLength(500)
      .WithMessage("A URL da Collection deve ter no máximo 500 caracteres.")
      .Must(BeAValidUrl)
      .WithMessage("A URL da Collection não é válida.");
  }

  private static bool BeAValidUrl(string url) =>
    Uri.TryCreate(url, UriKind.Absolute, out _);
}
