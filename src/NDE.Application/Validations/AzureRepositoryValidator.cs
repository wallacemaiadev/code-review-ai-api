
using FluentValidation;

using NDE.Domain.Entities.CodeReviews;

namespace NDE.Application.Validators;

public class AzureRepositoryValidator : AbstractValidator<AzureRepository>
{
  public AzureRepositoryValidator()
  {
    RuleFor(x => x.Id)
        .NotEmpty()
        .WithMessage("O Id do repositório é obrigatório.");

    RuleFor(x => x.RepositoryName)
        .NotEmpty()
        .WithMessage("O nome do repositório é obrigatório.")
        .MaximumLength(200)
        .WithMessage("O nome do repositório deve ter no máximo 200 caracteres.");

    RuleFor(x => x.RepositoryUrl)
        .NotEmpty()
        .WithMessage("A URL do repositório é obrigatória.")
        .MaximumLength(500)
        .WithMessage("A URL do repositório deve ter no máximo 500 caracteres.")
        .Must(BeAValidUrl)
        .WithMessage("A URL do repositório não é válida.");
  }

  private static bool BeAValidUrl(string url) =>
      Uri.TryCreate(url, UriKind.Absolute, out _);
}
