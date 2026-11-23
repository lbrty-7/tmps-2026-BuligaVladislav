namespace Bookstore.Notifications;

// TARGET of the Adapter: the interface the bookstore wants for any channel.
public interface INotificationChannel
{
    string Name { get; }
    DeliveryResult Send(string recipient, string subject, string body);
}

public record DeliveryResult(bool Delivered, string Info);
