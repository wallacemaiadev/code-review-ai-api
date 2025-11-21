
using FluentValidation;

using NDE.Domain.Entities.CodeReviews;

namespace NDE.Application.Validators;

public class CodeReviewValidator : AbstractValidator<CodeReview>
{
  public CodeReviewValidator()
  {
    RuleFor(x => x.FilePath)
        .NotEmpty()
        .WithMessage("O caminho do arquivo é obrigatório.")
        .MaximumLength(500)
        .WithMessage("O caminho do arquivo deve ter no máximo 500 caracteres.");

    RuleFor(x => x.Suggestion)
        .NotEmpty()
        .WithMessage("A sugestão da IA é obrigatória.");

    RuleFor(x => x.VerdictId)
        .IsInEnum()
        .WithMessage("O Verdict deve ter um valor válido.");

    RuleFor(x => x.TokensConsumed)
        .GreaterThanOrEqualTo(0)
        .WithMessage("TokensConsumidos deve ser maior ou igual a zero.");
  }
}
