namespace NDE.Domain.Notifications;

public interface INotificator
{
  bool HasNotification();
  List<Notification> GetNotifications();
  void Handle(Notification notificacao);
}
