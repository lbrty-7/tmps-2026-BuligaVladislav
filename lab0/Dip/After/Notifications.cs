namespace SolidPrinciples.Dip.After;

// The abstraction is owned by the high-level module.
public interface IMessageSender
{
    void Send(string to, string text);
}

// Low-level details depend on the abstraction.
public class SmtpEmailSender : IMessageSender
{
    public void Send(string to, string text) =>
        Console.WriteLine($"  [SMTP] connecting to smtp.bookstore.md ... sent to {to}: {text}");
}

public class SmsSender : IMessageSender
{
    public void Send(string to, string text) =>
        Console.WriteLine($"  [SMS]  to {to}: {text}");
}

// Test double: records messages instead of sending them.
public class FakeSender : IMessageSender
{
    public List<string> Sent { get; } = new();
    public void Send(string to, string text) => Sent.Add($"{to}|{text}");
}

// AFTER (follows DIP)
// The service receives its sender through the constructor
// (constructor injection) and never knows which concrete class it got.
public class OrderNotificationService
{
    private readonly IMessageSender _sender;

    public OrderNotificationService(IMessageSender sender) => _sender = sender;

    public void NotifyShipped(string customer, string book) =>
        _sender.Send(customer, $"'{book}' has been shipped");
}
