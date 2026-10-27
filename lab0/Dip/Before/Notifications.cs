namespace SolidPrinciples.Dip.Before;

// Low-level detail: a concrete e-mail sender.
public class SmtpEmailSender
{
    public void Send(string to, string text) =>
        Console.WriteLine($"  [SMTP] connecting to smtp.bookstore.md ... sent to {to}: {text}");
}

// BEFORE (violates DIP)
// The high-level policy (when and what to notify) creates and depends on
// the concrete low-level class. Switching to SMS, or testing without
// a real mail server, means rewriting this class.
public class OrderNotificationService
{
    private readonly SmtpEmailSender _sender = new();

    public void NotifyShipped(string customer, string book) =>
        _sender.Send(customer, $"'{book}' has been shipped");
}
