using System.Globalization;
using Bookstore.Checkout;
using Bookstore.Creation;
using Bookstore.Domain;
using Bookstore.Notifications;
using Bookstore.Pricing;

// the client of the whole system

// same number format on every computer (350.00, not 350,00)
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
Console.OutputEncoding = System.Text.Encoding.UTF8;

static void Title(string text)
{
    Console.WriteLine();
    Console.WriteLine(new string('=', 64));
    Console.WriteLine(text);
    Console.WriteLine(new string('=', 64));
}

const string CleanCode = "978-0132350884";
const string DesignPatterns = "978-0201633610";
const string EffectiveJava = "978-0134685991";

// e-mails are written as .eml files into this folder (no mail server needed)
string outbox = Path.Combine(AppContext.BaseDirectory, "outbox");
if (Directory.Exists(outbox)) Directory.Delete(outbox, recursive: true);

// ---------------------------------------------------------------- Adapter
Title("1. ADAPTER - System.Net.Mail.SmtpClient behind INotificationChannel");

using var smtpChannel = SmtpEmailChannel.ToPickupDirectory(outbox, "orders@librariafaf.md");
INotificationChannel channel = smtpChannel;

var sent = channel.Send("ana@mail.md", "Welcome to Libraria FAF", "Your account is ready.");
Console.WriteLine($"  Send(\"ana@mail.md\", ...)   -> Delivered={sent.Delivered}: {sent.Info}");

string eml = Directory.GetFiles(outbox, "*.eml").Single();
Console.WriteLine($"  SmtpClient wrote one .eml file into {Path.GetFileName(outbox)}/ with the headers:");
foreach (string line in File.ReadLines(eml).Where(l => l.StartsWith("From:") || l.StartsWith("To:") || l.StartsWith("Subject:")))
    Console.WriteLine($"    {line}");

var bad = channel.Send("not-an-email", "Test", "Test");
Console.WriteLine($"  Send(\"not-an-email\", ...)  -> Delivered={bad.Delivered}: {bad.Info}");

// ---------------------------------------------------------------- Decorator
Title("2. DECORATOR - extras wrapped around the price");

var cart = new Cart().Add(CleanCode, "Clean Code", 350m).Add(DesignPatterns, "Design Patterns", 400m);

IOrderCost plain = new BooksCost(cart);
IOrderCost gift = new GiftWrapDecorator(plain);
IOrderCost full = new StudentDiscountDecorator(
                      new InsuranceDecorator(
                          new ExpressDeliveryDecorator(
                              new GiftWrapDecorator(
                                  new BooksCost(cart)))));

foreach (var c in new[] { plain, gift, full })
    Console.WriteLine($"  {c.Cost(),7:0.00} MDL  <- {c.Description()}");

Console.WriteLine();
Console.WriteLine("  The order of the wrappers matters for percentages:");
IOrderCost discountFirst = new ExpressDeliveryDecorator(new StudentDiscountDecorator(new BooksCost(cart)));
IOrderCost discountLast = new StudentDiscountDecorator(new ExpressDeliveryDecorator(new BooksCost(cart)));
Console.WriteLine($"  {discountFirst.Cost(),7:0.00} MDL  <- {discountFirst.Description()}");
Console.WriteLine($"  {discountLast.Cost(),7:0.00} MDL  <- {discountLast.Description()}");

// ---------------------------------------------------------------- Facade
Title("3. FACADE - the whole checkout in one call");
Console.WriteLine("  path: client -> CheckoutFacade -> Decorator chain -> payment -> shipping");
Console.WriteLine("        -> INotificationChannel (SmtpEmailChannel adapter) -> SmtpClient");
Console.WriteLine();

var inventory = new InventoryService(new Dictionary<string, int>
{
    [CleanCode] = 5,
    [DesignPatterns] = 2,
    [EffectiveJava] = 0,
});
var checkout = new CheckoutFacade(inventory, new ShippingService(), channel);

void Run(string label, Cart c, CheckoutRequest request)
{
    Console.WriteLine(label);
    var result = checkout.PlaceOrder(c, request);
    Console.WriteLine($"  => {(result.Success ? "SUCCESS" : "FAILED")}: {result.Details}");
    Console.WriteLine();
}

// the requests are created with the Builder from Lab 1
Run("a) gift order with student discount",
    new Cart().Add(CleanCode, "Clean Code", 350m, 2),
    new CheckoutRequestBuilder()
        .For("ana@mail.md")
        .ShipTo("str. Studenților 9/7, Chișinău")
        .PayWith(new CardPayment("4111-0000-0000-1234"))
        .With(c => new GiftWrapDecorator(c))
        .With(c => new StudentDiscountDecorator(c))
        .Build());

Run("b) express order with insurance",
    new Cart().Add(DesignPatterns, "Design Patterns", 400m),
    new CheckoutRequestBuilder()
        .For("ion@mail.md")
        .ShipTo("bd. Dacia 41, Chișinău")
        .PayWith(new CardPayment("5555-0000-0000-4444"))
        .With(c => new ExpressDeliveryDecorator(c))
        .With(c => new InsuranceDecorator(c))
        .Build());

Run("c) a book that is sold out",
    new Cart().Add(EffectiveJava, "Effective Java", 420m),
    new CheckoutRequestBuilder()
        .For("maria@mail.md")
        .ShipTo("str. Pușkin 22, Chișinău")
        .PayWith(new CardPayment("4111-0000-0000-1234"))
        .Build());

Run("d) declined card: the reservation is rolled back",
    new Cart().Add(DesignPatterns, "Design Patterns", 400m),
    new CheckoutRequestBuilder()
        .For("vlad@mail.md")
        .ShipTo("str. Studenților 9/7, Chișinău")
        .PayWith(new CardPayment("4000-0000-0000-0002"))
        .Build());

Console.WriteLine($"Stock left: Clean Code {inventory.InStock(CleanCode)}, " +
                  $"Design Patterns {inventory.InStock(DesignPatterns)}, " +
                  $"Effective Java {inventory.InStock(EffectiveJava)}");
Console.WriteLine($"E-mails in the outbox: {Directory.GetFiles(outbox, "*.eml").Length}");
