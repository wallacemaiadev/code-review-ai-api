using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

using NDE.Application.ViewModels.Response;
using NDE.Domain.Notifications;

namespace NDE.Api.Controllers;

[ApiController]
public abstract class MainController : ControllerBase
{
  private readonly INotificator _notificator;

  protected MainController(INotificator notificator)
  {
    _notificator = notificator;
  }

  protected bool ValidOperation() => !_notificator.HasNotification();

  protected ActionResult NDEResponse<TResult>(TResult? result = default)
  {
    if (ValidOperation())
    {
      if (result is null)
        return NoContent();

      return Ok(NDEResponseModel<TResult>.Ok(result));
    }

    return BadRequest(NDEResponseModel<TResult>.Fail(
        _notificator.GetNotifications().Select(n => n.Message)
    ));
  }

  protected ActionResult NDEResponse()
  {
    if (ValidOperation())
    {
      return NoContent();
    }

    return BadRequest(NDEResponseModel<object?>.Fail(
        _notificator.GetNotifications().Select(n => n.Message)
    ));
  }

  protected ActionResult NDEResponse(ModelStateDictionary modelState)
  {
    if (!modelState.IsValid) HandleModelErrors(modelState);
    return NDEResponse<object?>(null);
  }

  private void HandleModelErrors(ModelStateDictionary modelState)
  {
    foreach (var error in modelState.Values.SelectMany(x => x.Errors))
    {
      var message = error.Exception?.Message ?? error.ErrorMessage;
      NotifyError(message);
    }
  }

  protected void NotifyError(string message) =>
    _notificator.Handle(new Notification(message));
}