using System.Globalization;
using SrpBefore = SolidPrinciples.Srp.Before;
using SrpAfter = SolidPrinciples.Srp.After;
using OcpBefore = SolidPrinciples.Ocp.Before;
using OcpAfter = SolidPrinciples.Ocp.After;
using DipBefore = SolidPrinciples.Dip.Before;
using DipAfter = SolidPrinciples.Dip.After;

// same number format on every computer (350.00, not 350,00)
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

static void Title(string text)
{
    Console.WriteLine();
    Console.WriteLine(new string('=', 60));
    Console.WriteLine(text);
    Console.WriteLine(new string('=', 60));
}

// ---------------- S: Single Responsibility ----------------
Title("S - Single Responsibility Principle");

Console.WriteLine("Before: one OrderProcessor does everything");
new SrpBefore.OrderProcessor().Process("ana@mail.md", "Clean Code", 2, 350m);

Console.WriteLine("After: OrderService coordinates small classes");
new SrpAfter.OrderService().Place(new SrpAfter.Order("ana@mail.md", "Clean Code", 2, 350m));

// ---------------- O: Open/Closed ----------------
Title("O - Open/Closed Principle");

const decimal amount = 800m;
Console.WriteLine("Before: switch on a string");
var oldCalc = new OcpBefore.DiscountCalculator();
foreach (var type in new[] { "Regular", "Student", "Vip" })
    Console.WriteLine($"  {type,-12} {amount} -> {oldCalc.Apply(type, amount):0.00}");
try { oldCalc.Apply("BlackFriday", amount); }
catch (ArgumentException e) { Console.WriteLine($"  BlackFriday  -> ERROR: {e.Message} (class must be edited)"); }

Console.WriteLine("After: one class per policy, new policy added without edits");
var newCalc = new OcpAfter.DiscountCalculator();
OcpAfter.IDiscountPolicy[] policies =
{
    new OcpAfter.RegularDiscount(), new OcpAfter.StudentDiscount(),
    new OcpAfter.VipDiscount(), new OcpAfter.BlackFridayDiscount()
};
foreach (var p in policies)
    Console.WriteLine($"  {p.Name,-12} {amount} -> {newCalc.Apply(p, amount):0.00}");

// ---------------- D: Dependency Inversion ----------------
Title("D - Dependency Inversion Principle");

Console.WriteLine("Before: service creates SmtpEmailSender itself");
new DipBefore.OrderNotificationService().NotifyShipped("ana@mail.md", "Clean Code");

Console.WriteLine("After: sender is injected through an interface");
new DipAfter.OrderNotificationService(new DipAfter.SmtpEmailSender()).NotifyShipped("ana@mail.md", "Clean Code");
new DipAfter.OrderNotificationService(new DipAfter.SmsSender()).NotifyShipped("+37369000000", "Clean Code");

var fake = new DipAfter.FakeSender();
new DipAfter.OrderNotificationService(fake).NotifyShipped("test@mail.md", "Clean Code");
Console.WriteLine($"  [TEST] fake sender recorded {fake.Sent.Count} message: {fake.Sent[0]}");
