using FluentValidation;
using FluentValidation.Results;

using NDE.Domain.Notifications;

namespace NDE.Application.Services;

public class BaseService
{
  private readonly INotificator _notificador;

  protected BaseService(INotificator notificator)
  {
    _notificador = notificator;
  }

  protected void Notify(ValidationResult validationResult)
  {
    foreach (var error in validationResult.Errors)
    {
      Notify(error.ErrorMessage);
    }
  }

  protected void Notify(string message)
  {
    _notificador.Handle(new Notification(message));
  }
  protected bool Commit(bool isValid)
  {
    if (!isValid)
    {
      Notify("Houve um erro na sua solicitação, entre em contato com o administrador do sistema.");
      return false;
    }

    return true;
  }
  protected bool Validate<TE>(AbstractValidator<TE> validator, TE entity)
  {
    var result = validator.Validate(entity);
    if (result.IsValid) return true;
    Notify(result);
    return false;
  }
}