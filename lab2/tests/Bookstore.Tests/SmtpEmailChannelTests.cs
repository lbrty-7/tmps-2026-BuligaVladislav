using Bookstore.Notifications;

namespace Bookstore.Tests;

// Adapter: SmtpEmailChannel must translate our Send(recipient, subject, body)
// into what System.Net.Mail.SmtpClient needs, and translate its errors back.
public class SmtpEmailChannelTests : IDisposable
{
    private readonly string _outbox = Path.Combine(Path.GetTempPath(), "outbox-" + Guid.NewGuid().ToString("N"));

    public void Dispose()
    {
        if (Directory.Exists(_outbox)) Directory.Delete(_outbox, recursive: true);
    }

    [Fact]
    public void Send_TranslatesTheCallIntoAnEmailWrittenBySmtpClient()
    {
        using var adapter = SmtpEmailChannel.ToPickupDirectory(_outbox, "orders@librariafaf.md");
        INotificationChannel channel = adapter;

        DeliveryResult result = channel.Send("ana@mail.md", "Order confirmed", "Total 652.50 MDL");

        Assert.True(result.Delivered);
        string eml = Assert.Single(Directory.GetFiles(_outbox, "*.eml"));
        string text = File.ReadAllText(eml);
        Assert.Contains("To: ana@mail.md", text);
        Assert.Contains("Subject: Order confirmed", text);
        Assert.Contains("<orders@librariafaf.md>", text);
    }

    [Fact]
    public void Send_InvalidAddress_ReturnsFailureInsteadOfThrowing()
    {
        using var adapter = SmtpEmailChannel.ToPickupDirectory(_outbox, "orders@librariafaf.md");

        DeliveryResult result = adapter.Send("not-an-email", "Order confirmed", "...");

        Assert.False(result.Delivered);
        Assert.Contains("not a valid e-mail address", result.Info);
        Assert.Empty(Directory.GetFiles(_outbox, "*.eml"));
    }
}
