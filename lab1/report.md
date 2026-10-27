# Report on laboratory work no. 1

**Course:** Software Design Techniques and Mechanisms (TMPS)  
**Topic:** SOLID Principles in C#  
**Done by:** st. gr. FAF-243, Buliga Vladislav  
**Checked by:** Istrati Daniel, asist. univ.  
Chișinău, 2026

---


## 1 AIM OF THE WORK


### 1.1 Purpose of the laboratory work

The purpose of this laboratory work is to understand three of the five SOLID principles of object-oriented design and to see, on real code, what changes when a program follows them. For each principle I first wrote a small "before" version that breaks the principle on purpose, and then an "after" version of the same feature that respects it. Both versions are in the same C# console project, so they can be run one after another and compared side by side.


### 1.2 Problem statement

**Tasks of the laboratory work:**

- choose three SOLID principles and explain each of them in my own words;
- for each principle, write a C# example that violates it (before) and a refactored example that follows it (after);
- keep the behaviour of the program the same, so the difference is only in the structure of the code;
- run both versions and show the output;
- compare the versions and draw conclusions about maintainability, extensibility and testability.

The three chosen principles are the Single Responsibility Principle (S), the Open/Closed Principle (O) and the Dependency Inversion Principle (D). All examples use the same small domain, an online bookstore that takes orders, applies discounts and notifies customers.


## 2 THEORETICAL BACKGROUND


### 2.1 The SOLID Principles

SOLID is an acronym for five design principles collected by Robert C. Martin: Single Responsibility, Open/Closed, Liskov Substitution, Interface Segregation and Dependency Inversion. They do not add new features to a program; they describe how to split code into classes and how these classes should depend on each other, so that the program stays easy to change when the requirements change. Most of them are about the same idea seen from different sides: keeping things that change for different reasons apart, and depending on abstractions (interfaces) instead of concrete classes.


### 2.2 Single Responsibility Principle (SRP)

SRP says that a class should have only one reason to change. "Responsibility" here does not mean "one method", it means one job, or one actor that can ask for changes. If one class validates data, calculates prices, saves to the database and sends e-mails, then a change in the VAT rate, in the database or in the e-mail text all touch the same file. Such a class becomes long, hard to read and risky to modify. When the jobs are separated into small classes, each change stays local and each class can be understood and tested on its own.


### 2.3 Open/Closed Principle (OCP)

OCP, formulated by Bertrand Meyer, says that software entities should be open for extension but closed for modification. In practice this means that adding a new behaviour should be done by writing new code, not by editing code that already works and is already tested. The usual sign of an OCP violation is a long if/else or switch over a type code: every new case forces us to open the same method again. The usual fix is polymorphism: we define an interface for the varying behaviour and put every variant in its own class.


### 2.4 Dependency Inversion Principle (DIP)

DIP has two parts: high-level modules should not depend on low-level modules, both should depend on abstractions; and abstractions should not depend on details, details should depend on abstractions. A high-level module contains the business rules (for example "notify the customer when the book is shipped"), while a low-level module is a technical detail (an SMTP client, an SMS gateway, a file). If the business class creates the detail with new, it is tied to it forever. With DIP the business class only knows an interface, and the concrete class is given to it from outside.


### 2.5 Dependency Injection and Interfaces in C#

Dependency injection is the common technique used to apply DIP: the object receives its dependencies, usually through the constructor, instead of creating them. In C# the abstraction is normally an interface (by convention its name starts with I, like IMessageSender). A class implements the interface with the : syntax, and any object of a class that implements it can be passed where the interface is expected. This also makes testing easy, because a fake implementation can be passed instead of the real one.


## 3 IMPLEMENTATION

The project was written in C# 12 on .NET 8 as a single console application called SolidPrinciples. Each principle has its own folder with a Before and an After subfolder, and each version has its own namespace, so the two versions of a class can have the same name without conflicts. Program.cs runs all six versions in order and prints what each one does.

*Listing 3.1 – Structure of the project*

```
lab1/
├── SolidPrinciples.csproj
├── Program.cs                     demo runner
├── Srp/Before/OrderProcessor.cs
├── Srp/After/Order.cs             Order, OrderValidator,
│                                  PriceCalculator, OrderRepository,
│                                  EmailNotifier, InvoicePrinter
├── Srp/After/OrderService.cs
├── Ocp/Before/DiscountCalculator.cs
├── Ocp/After/Discounts.cs
├── Dip/Before/Notifications.cs
└── Dip/After/Notifications.cs
```

The program is built and run from the lab1 folder with the command below. It does not use any NuGet packages, only the standard library.

*Listing 3.2 – Running the program*

```bash
dotnet run
```


### 3.1 Single Responsibility Principle

**Before**

The first version has one class, OrderProcessor, with one method Process. The method checks the e-mail and the quantity, calculates the total with 20% VAT, saves the order in a list that plays the role of a database, "sends" an e-mail and prints the invoice. The code works, but the class has five different reasons to change: the validation rules, the tax, the storage, the e-mail text and the invoice format. For example, to change the invoice layout I would have to edit the same method that saves orders, and I could break the saving by mistake. It is also impossible to test the price calculation alone, because calling Process also saves and sends e-mails.

*Listing 3.3 – Srp/Before/OrderProcessor.cs (violates SRP)*

```csharp
namespace SolidPrinciples.Srp.Before;

// BEFORE (violates SRP)
// One class does everything: it checks the order, calculates the price,
// saves the order, sends the e-mail and prints the invoice.
// It has five different reasons to change.
public class OrderProcessor
{
    private readonly List<string> _database = new();

    public void Process(string customerEmail, string book, int quantity, decimal unitPrice)
    {
        // 1. validation
        if (string.IsNullOrWhiteSpace(customerEmail) || !customerEmail.Contains('@'))
            throw new ArgumentException("Invalid e-mail");
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive");

        // 2. price calculation (VAT 20%)
        decimal total = quantity * unitPrice * 1.20m;

        // 3. persistence
        _database.Add($"{customerEmail};{book};{quantity};{total}");
        Console.WriteLine($"  [DB]    saved order #{_database.Count}");

        // 4. notification
        Console.WriteLine($"  [EMAIL] to {customerEmail}: your order for '{book}' was received");

        // 5. invoice formatting
        Console.WriteLine($"  [INVOICE] {quantity} x {book} @ {unitPrice:0.00} = {total:0.00} MDL (VAT incl.)");
    }
}
```

**After**

In the refactored version the five jobs were moved into five small classes: OrderValidator only knows the rules of a valid order, PriceCalculator only knows how to calculate the price and keeps the VAT in one constant, OrderRepository only stores orders, EmailNotifier only talks to the customer and InvoicePrinter only formats the invoice. The data was also moved into a separate record Order, so the method no longer receives four loose parameters. OrderService does not do any work itself: it only calls the other classes in the right order.

*Listing 3.4 – Srp/After/Order.cs (one responsibility per class)*

```csharp
namespace SolidPrinciples.Srp.After;

// Plain data: what an order is.
public record Order(string CustomerEmail, string Book, int Quantity, decimal UnitPrice);

// Only knows the rules of a valid order.
public class OrderValidator
{
    public void Validate(Order order)
    {
        if (string.IsNullOrWhiteSpace(order.CustomerEmail) || !order.CustomerEmail.Contains('@'))
            throw new ArgumentException("Invalid e-mail");
        if (order.Quantity <= 0)
            throw new ArgumentException("Quantity must be positive");
    }
}

// Only knows how to calculate the price.
public class PriceCalculator
{
    private const decimal Vat = 0.20m;

    public decimal Total(Order order) => order.Quantity * order.UnitPrice * (1 + Vat);
}

// Only knows how to store orders.
public class OrderRepository
{
    private readonly List<Order> _orders = new();

    public int Save(Order order)
    {
        _orders.Add(order);
        Console.WriteLine($"  [DB]    saved order #{_orders.Count}");
        return _orders.Count;
    }
}

// Only knows how to tell the customer.
public class EmailNotifier
{
    public void OrderReceived(Order order) =>
        Console.WriteLine($"  [EMAIL] to {order.CustomerEmail}: your order for '{order.Book}' was received");
}

// Only knows how an invoice looks.
public class InvoicePrinter
{
    public void Print(Order order, decimal total) =>
        Console.WriteLine($"  [INVOICE] {order.Quantity} x {order.Book} @ {order.UnitPrice:0.00} = {total:0.00} MDL (VAT incl.)");
}
```

*Listing 3.5 – Srp/After/OrderService.cs (coordinates the steps)*

```csharp
namespace SolidPrinciples.Srp.After;

// AFTER (follows SRP)
// The service only coordinates the steps; each step lives in its own class
// with a single reason to change.
public class OrderService
{
    private readonly OrderValidator _validator = new();
    private readonly PriceCalculator _calculator = new();
    private readonly OrderRepository _repository = new();
    private readonly EmailNotifier _notifier = new();
    private readonly InvoicePrinter _printer = new();

    public void Place(Order order)
    {
        _validator.Validate(order);
        decimal total = _calculator.Total(order);
        _repository.Save(order);
        _notifier.OrderReceived(order);
        _printer.Print(order, total);
    }
}
```

Now, if the VAT changes, only PriceCalculator is edited; if the e-mail text changes, only EmailNotifier is edited. Each class is a few lines long and can be tested alone. The output of both versions is exactly the same (Listing 3.11), which shows that only the structure changed, not the behaviour.


### 3.2 Open/Closed Principle

**Before**

The first DiscountCalculator receives the customer type as a string and uses a switch to decide the discount: Regular pays the full price, Student gets 10% and Vip gets 20%. When the shop wants a new Black Friday discount, there is no way to add it without opening this class and adding another case. Every new discount means editing, recompiling and retesting code that already worked. A typo in the string (for example "vip" instead of "Vip") is also only found at run time.

*Listing 3.6 – Ocp/Before/DiscountCalculator.cs (violates OCP)*

```csharp
namespace SolidPrinciples.Ocp.Before;

// BEFORE (violates OCP)
// Every new customer type means opening this class and adding another case.
// The class is never "closed": each change risks breaking the old discounts.
public class DiscountCalculator
{
    public decimal Apply(string customerType, decimal amount)
    {
        switch (customerType)
        {
            case "Regular":
                return amount;
            case "Student":
                return amount * 0.90m;          // 10% off
            case "Vip":
                return amount * 0.80m;          // 20% off
            default:
                throw new ArgumentException($"Unknown customer type: {customerType}");
        }
    }
}
```

**After**

In the new version each discount is a separate class that implements the interface IDiscountPolicy, which has a Name and an Apply method. The calculator receives an IDiscountPolicy and simply calls Apply on it, so it does not know which discounts exist. To show that the code is really closed for modification, the BlackFridayDiscount class (40% off orders over 500 MDL, 25% otherwise) was added after the other three, without changing a single line in DiscountCalculator or in the existing discounts.

*Listing 3.7 – Ocp/After/Discounts.cs (open for extension, closed for modification)*

```csharp
namespace SolidPrinciples.Ocp.After;

// AFTER (follows OCP)
// The calculator depends on an abstraction. New discounts are added
// as new classes; the calculator itself is never edited again.
public interface IDiscountPolicy
{
    string Name { get; }
    decimal Apply(decimal amount);
}

public class RegularDiscount : IDiscountPolicy
{
    public string Name => "Regular";
    public decimal Apply(decimal amount) => amount;
}

public class StudentDiscount : IDiscountPolicy
{
    public string Name => "Student";
    public decimal Apply(decimal amount) => amount * 0.90m;
}

public class VipDiscount : IDiscountPolicy
{
    public string Name => "Vip";
    public decimal Apply(decimal amount) => amount * 0.80m;
}

// Added later as an extension: no existing class was modified.
public class BlackFridayDiscount : IDiscountPolicy
{
    public string Name => "BlackFriday";
    public decimal Apply(decimal amount) => amount > 500m ? amount * 0.60m : amount * 0.75m;
}

public class DiscountCalculator
{
    public decimal Apply(IDiscountPolicy policy, decimal amount) => policy.Apply(amount);
}
```

In the output the old calculator throws an exception for BlackFriday, because the case does not exist, while the new one handles all four policies. The type is now checked by the compiler: it is not possible to pass a policy that does not exist.


### 3.3 Dependency Inversion Principle

**Before**

The first OrderNotificationService creates a SmtpEmailSender with new inside the class. The high-level rule "tell the customer that the book was shipped" is glued to a low-level detail, the SMTP client. If the shop wants to send an SMS instead, or both, the service has to be rewritten. It also cannot be unit-tested, because every call would try to send a real e-mail.

*Listing 3.8 – Dip/Before/Notifications.cs (violates DIP)*

```csharp
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
```

**After**

In the refactored version the service depends only on the interface IMessageSender, and it receives the concrete sender through its constructor (constructor injection). SmtpEmailSender, SmsSender and FakeSender all implement the interface, so the arrows of dependency are inverted: the details now depend on the abstraction that belongs to the high-level module, not the other way around. The same service class was used with three different senders without any change, including a FakeSender that only records the messages in a list, which is exactly what a unit test needs.

*Listing 3.9 – Dip/After/Notifications.cs (depends on an abstraction)*

```csharp
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
```


### 3.4 The Demo Program

Program.cs runs the before and after versions of every principle. The two versions live in different namespaces, so they are imported with aliases (SrpBefore, SrpAfter and so on).

*Listing 3.10 – Program.cs*

```csharp
using SrpBefore = SolidPrinciples.Srp.Before;
using SrpAfter = SolidPrinciples.Srp.After;
using OcpBefore = SolidPrinciples.Ocp.Before;
using OcpAfter = SolidPrinciples.Ocp.After;
using DipBefore = SolidPrinciples.Dip.Before;
using DipAfter = SolidPrinciples.Dip.After;

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
```

The output of dotnet run is shown below.

*Listing 3.11 – Output of the program*

```

============================================================
S - Single Responsibility Principle
============================================================
Before: one OrderProcessor does everything
  [DB]    saved order #1
  [EMAIL] to ana@mail.md: your order for 'Clean Code' was received
  [INVOICE] 2 x Clean Code @ 350.00 = 840.00 MDL (VAT incl.)
After: OrderService coordinates small classes
  [DB]    saved order #1
  [EMAIL] to ana@mail.md: your order for 'Clean Code' was received
  [INVOICE] 2 x Clean Code @ 350.00 = 840.00 MDL (VAT incl.)

============================================================
O - Open/Closed Principle
============================================================
Before: switch on a string
  Regular      800 -> 800.00
  Student      800 -> 720.00
  Vip          800 -> 640.00
  BlackFriday  -> ERROR: Unknown customer type: BlackFriday (class must be edited)
After: one class per policy, new policy added without edits
  Regular      800 -> 800.00
  Student      800 -> 720.00
  Vip          800 -> 640.00
  BlackFriday  800 -> 480.00

============================================================
D - Dependency Inversion Principle
============================================================
Before: service creates SmtpEmailSender itself
  [SMTP] connecting to smtp.bookstore.md ... sent to ana@mail.md: 'Clean Code' has been shipped
After: sender is injected through an interface
  [SMTP] connecting to smtp.bookstore.md ... sent to ana@mail.md: 'Clean Code' has been shipped
  [SMS]  to +37369000000: 'Clean Code' has been shipped
  [TEST] fake sender recorded 1 message: test@mail.md|'Clean Code' has been shipped
```

For SRP both versions print the same three lines, so the refactoring did not change the behaviour. For OCP the old calculator fails on the new BlackFriday type, while the new one applies it (800 MDL → 480 MDL). For DIP the same service sends through SMTP, through SMS and into a fake sender, and the fake sender recorded exactly one message.


## 4 RESULTS

Table 4.1 sums up what changed for each principle.

*Table 4.1 – Comparison of the versions before and after refactoring*

| Principle | Before | After | What we gain |
|---|---|---|---|
| SRP | OrderProcessor does validation, pricing, storage, e-mail and invoice (5 reasons to change) | 6 small classes + OrderService that only coordinates | Changes stay local; each class can be tested alone |
| OCP | switch on a string; a new discount needs editing the class | IDiscountPolicy + one class per discount | BlackFriday added without touching old code; type checked by compiler |
| DIP | Service creates SmtpEmailSender with new | Service receives IMessageSender in the constructor | Senders can be swapped (SMTP, SMS, fake); easy unit tests |

The refactored versions have more classes and slightly more lines of code, which is the price of these principles. For a tiny program this can look like too much, but every new requirement (a new tax rule, a new discount, a new notification channel) now costs one new or one changed small class instead of editing a central class that does everything.


## CONCLUSION

In this laboratory work I applied three SOLID principles in C# and compared, for each one, a version that breaks the principle with a version that follows it. With the Single Responsibility Principle, one OrderProcessor that had five reasons to change was split into a validator, a price calculator, a repository, a notifier and an invoice printer, coordinated by a small OrderService. The program printed exactly the same output as before, but now a change in the VAT or in the e-mail text touches only one small class.

With the Open/Closed Principle the switch over customer types was replaced by the IDiscountPolicy interface and one class per discount. The best proof that it works was adding the Black Friday discount: in the old version it threw an exception until the class was edited, while in the new version it was added as a new class without modifying any existing code. With the Dependency Inversion Principle the notification service stopped creating an SMTP client itself and started receiving an IMessageSender through its constructor. The same service then worked with e-mail, SMS and a fake sender for tests.

The main thing I understood is that these principles work together: SRP gives small classes, OCP and DIP connect them through interfaces, and as a result the code is easier to extend and to test. They also have a cost, more classes and more files, so they should be applied where the code is really expected to change, not everywhere by default.


## REFERENCES

1. Martin, R. C. Agile Software Development, Principles, Patterns, and Practices. Prentice Hall, 2002. Chapters 8–11 (SRP, OCP, LSP, DIP).

2. Martin, R. C. Clean Architecture: A Craftsman's Guide to Software Structure and Design. Prentice Hall, 2017. Part III "Design Principles".

3. Meyer, B. Object-Oriented Software Construction. 2nd ed. Prentice Hall, 1997 (the Open/Closed Principle).

4. Gamma, E., Helm, R., Johnson, R., Vlissides, J. Design Patterns: Elements of Reusable Object-Oriented Software. Addison-Wesley, 1994.

5. Microsoft. Architectural principles – .NET. Available at: https://learn.microsoft.com/dotnet/architecture/modern-web-apps-azure/architectural-principles

6. Microsoft. Interfaces – define behavior for multiple types (C# guide). Available at: https://learn.microsoft.com/dotnet/csharp/fundamentals/types/interfaces

