using System.Net.Mail;
using System.Text;

namespace Bookstore.Notifications;

// ADAPTER
// Adaptee: System.Net.Mail.SmtpClient, a .NET library class we did not write
// and cannot change. It wants a MailMessage object built from MailAddress
// objects and reports problems by throwing exceptions.
// The adapter turns our simple Send(recipient, subject, body) into that,
// and turns the exceptions back into a DeliveryResult.
public sealed class SmtpEmailChannel : INotificationChannel, IDisposable
{
    private readonly SmtpClient _smtp;
    private readonly MailAddress _from;

    public SmtpEmailChannel(SmtpClient smtp, string fromAddress)
    {
        _smtp = smtp;
        _from = new MailAddress(fromAddress, "Libraria FAF");
    }

    // Without a real mail server, SmtpClient can write each e-mail as an
    // .eml file into a folder (its "pickup directory" delivery mode).
    public static SmtpEmailChannel ToPickupDirectory(string directory, string fromAddress)
    {
        Directory.CreateDirectory(directory);
        var smtp = new SmtpClient
        {
            DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory,
            PickupDirectoryLocation = Path.GetFullPath(directory)
        };
        return new SmtpEmailChannel(smtp, fromAddress);
    }

    public string Name => "e-mail (SMTP)";

    public DeliveryResult Send(string recipient, string subject, string body)
    {
        try
        {
            using var message = new MailMessage(_from, new MailAddress(recipient))
            {
                Subject = subject,
                Body = body,
                BodyEncoding = Encoding.UTF8
            };
            _smtp.Send(message);
            return new DeliveryResult(true, $"e-mail to {recipient} handed to SMTP");
        }
        catch (FormatException)
        {
            return new DeliveryResult(false, $"\"{recipient}\" is not a valid e-mail address");
        }
        catch (SmtpException e)
        {
            return new DeliveryResult(false, "SMTP error: " + e.Message);
        }
    }

    public void Dispose() => _smtp.Dispose();
}
