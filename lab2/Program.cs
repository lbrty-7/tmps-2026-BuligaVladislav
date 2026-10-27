using System.Globalization;
using StructuralPatterns.Adapter;
using StructuralPatterns.Decorator;
using StructuralPatterns.Facade;
using StructuralPatterns.Models;

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

// ---------------------------------------------------------------- Adapter
Title("1. ADAPTER - two payment services behind one interface");

var bank = new MaibBankGateway();
var globalPay = new GlobalPayClient();

IPaymentProcessor[] processors =
{
    new MaibPaymentAdapter(bank, "4111-0000-0000-1234"),
    new GlobalPayAdapter(globalPay),
    new MaibPaymentAdapter(bank, "BLOCKED-0000-0000-9999"),
};

foreach (var p in processors)
{
    Console.WriteLine($"  {p.Name}: Pay(\"ana@mail.md\", 390.00 MDL)");
    var r = p.Pay("ana@mail.md", 390.00m);
    Console.WriteLine($"    -> Success={r.Success}, Ref={(r.Reference == "" ? "-" : r.Reference)}, {r.Message}");
}

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

var inventory = new InventoryService();
var checkout = new CheckoutFacade(inventory, new ShippingService(), new NotificationService());

void Run(string label, Cart c, CheckoutOptions o, IPaymentProcessor pay)
{
    Console.WriteLine($"{label}");
    var result = checkout.PlaceOrder(c, o, pay);
    Console.WriteLine($"  => {(result.Success ? "SUCCESS" : "FAILED")}: {result.Details}");
    Console.WriteLine();
}

Run("a) gift order paid by card",
    new Cart().Add(CleanCode, "Clean Code", 350m, 2),
    new CheckoutOptions("ana@mail.md", "str. Studenților 9/7, Chișinău", GiftWrap: true, Student: true),
    new MaibPaymentAdapter(bank, "4111-0000-0000-1234"));

Run("b) express order paid with GlobalPay",
    new Cart().Add(DesignPatterns, "Design Patterns", 400m),
    new CheckoutOptions("john@mail.com", "Hauptstraße 5, Berlin", Express: true, Insurance: true),
    new GlobalPayAdapter(globalPay));

Run("c) a book that is sold out",
    new Cart().Add(EffectiveJava, "Effective Java", 420m),
    new CheckoutOptions("ion@mail.md", "bd. Dacia 41, Chișinău"),
    new MaibPaymentAdapter(bank, "4111-0000-0000-1234"));

Run("d) blocked card: the reservation is rolled back",
    new Cart().Add(DesignPatterns, "Design Patterns", 400m),
    new CheckoutOptions("vlad@mail.md", "str. Studenților 9/7, Chișinău"),
    new MaibPaymentAdapter(bank, "BLOCKED-0000-0000-9999"));

Console.WriteLine($"Stock left: Clean Code {inventory.InStock(CleanCode)}, " +
                  $"Design Patterns {inventory.InStock(DesignPatterns)}, " +
                  $"Effective Java {inventory.InStock(EffectiveJava)}");
