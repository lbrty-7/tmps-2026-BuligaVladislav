# Report on laboratory work no. 2

**Course:** Software Design Techniques and Mechanisms (TMPS)  
**Topic:** Structural Design Patterns  
**Done by:** st. gr. FAF-243, Buliga Vladislav  
**Checked by:** Istrati Daniel, asist. univ.  
Chișinău, 2026

---


## 1 AIM OF THE WORK


### 1.1 Purpose of the laboratory work

The purpose of this laboratory work is to study the structural design patterns, which describe how classes and objects are combined into larger structures, and to implement three of them in one working project. Creational patterns answered the question "how is an object created"; structural patterns answer "how do objects fit together": how to connect a class that has the wrong interface, how to add behaviour to an object without changing its class, and how to hide a complicated group of classes behind one simple entry point.


### 1.2 Problem statement

**Tasks of the laboratory work:**

- study the structural design patterns: Adapter, Bridge, Composite, Decorator, Facade, Flyweight and Proxy;
- choose three of them and implement them in one project;
- make the patterns work together in the same domain, not as unrelated examples;
- run the program, show the output and explain how each pattern solves its problem.

The project continues the online bookstore from the first laboratory work and implements its checkout, in C# on .NET 8. The three implemented patterns are Adapter (two external payment services with incompatible interfaces), Decorator (optional extras added to the price of an order) and Facade (one simple method that runs the whole checkout).


## 2 THEORETICAL BACKGROUND


### 2.1 Structural Design Patterns

Structural patterns are the second group of the classic Gang of Four catalogue. They explain how to assemble classes and objects into larger structures while keeping these structures flexible and efficient. Most of them rely on composition, meaning that one object holds a reference to another and delegates work to it, instead of inheritance. Composition can be changed at run time and does not create the rigid class hierarchies that inheritance often produces. Several structural patterns look alike in code (an object that wraps another object), so the difference between them is mainly in their intent: what problem the wrapper solves.


### 2.2 Adapter

Adapter converts the interface of a class into another interface that the client expects, so that classes with incompatible interfaces can work together. It is used when we want to use an existing class, usually from a library or an external service that we cannot change, but its methods, parameters or results do not match the rest of our program. The adapter implements the interface expected by the client (the Target), holds a reference to the incompatible object (the Adaptee) and translates every call: it renames methods, converts units and data types and maps the results. The client code stays the same and does not know that an adapter is used.


### 2.3 Decorator

Decorator attaches additional responsibilities to an object dynamically. A decorator implements the same interface as the object it wraps, holds a reference to it and adds its own behaviour before or after forwarding the call. Because the decorator has the same interface, decorators can be stacked one inside another in any combination, and the client cannot tell a decorated object from a plain one. Decorator is a flexible alternative to subclassing: with four optional extras, inheritance would need a class for every combination (up to 16), while with decorators four small classes are enough. The order in which decorators are applied can change the result, which must be taken into account.


### 2.4 Facade

Facade provides a unified, simplified interface to a set of interfaces in a subsystem. A complex operation often needs several classes to be called in the right order, with checks and error handling between them. Instead of letting every client do this, a facade class offers one high-level method that does all the coordination. The subsystem classes are not hidden or changed and can still be used directly when needed; the facade only makes the common case simple and keeps the clients independent from the details of the subsystem.


### 2.5 The Other Structural Patterns

Bridge splits a large class or a set of closely related classes into two separate hierarchies, an abstraction and an implementation, which can be developed independently; for example, shapes and the renderers that draw them, so that adding a new shape or a new renderer does not multiply the number of classes. Composite composes objects into tree structures and lets clients treat individual objects and groups of objects in the same way, such as files and folders, or a single book and a box set of books that both have a price. Flyweight saves memory when a very large number of similar objects is needed, by sharing the common (intrinsic) state between them and keeping only the unique (extrinsic) state in each object, as text editors do for characters and fonts. Proxy provides a substitute for another object that controls access to it; it has the same interface as the real object and can add lazy loading (a virtual proxy), access checks (a protection proxy), caching or calls to a remote server (a remote proxy).


## 3 IMPLEMENTATION

The project is a C# 12 console application on .NET 8 called StructuralPatterns, using only the standard library. Each pattern has its own folder and namespace, and the shopping cart, which is shared by all of them, is in Models. Program.cs first shows each pattern separately and then runs four complete checkouts through the facade.

*Listing 3.1 – Structure of the project*

```
lab2/
├── StructuralPatterns.csproj
├── Program.cs                     demo of the three patterns
├── Models/Cart.cs                 CartItem, Cart
├── Adapter/ThirdPartyPayments.cs  MaibBankGateway, GlobalPayClient
├── Adapter/PaymentAdapters.cs     IPaymentProcessor + 2 adapters
├── Decorator/OrderCost.cs         IOrderCost, BooksCost, 4 decorators
├── Facade/Subsystems.cs           Inventory, Shipping, Notification
└── Facade/CheckoutFacade.cs       CheckoutFacade.PlaceOrder()
```

The program is built and run from the lab2 folder with the command below.

*Listing 3.2 – Running the program*

```bash
dotnet run
```

The three patterns are connected inside the checkout: the facade calculates the price by stacking decorators and pays through whichever adapter it receives, so the facade itself does not depend on any concrete extra or payment service.

*Listing 3.3 – Models/Cart.cs (the cart shared by all patterns)*

```csharp
namespace StructuralPatterns.Models;

public record CartItem(string Isbn, string Title, decimal Price, int Quantity)
{
    public decimal LineTotal => Price * Quantity;
}

public class Cart
{
    private readonly List<CartItem> _items = new();

    public IReadOnlyList<CartItem> Items => _items;

    public Cart Add(string isbn, string title, decimal price, int quantity = 1)
    {
        _items.Add(new CartItem(isbn, title, price, quantity));
        return this;
    }

    public decimal Subtotal => _items.Sum(i => i.LineTotal);
}
```


### 3.1 Adapter – Payment Services

The store accepts payments through two external services. Their classes simulate libraries written by other companies, so their code cannot be changed. MaibBankGateway, a Moldovan bank, has the method MakeTransfer(), takes the amount in bani (1 MDL = 100 bani) and returns a numeric code, where 0 means success and 51 means a blocked card, with the transfer id in an out parameter. GlobalPayClient, an international service, has the method SendPayment(), works only in euros, takes the customer's e-mail first and returns a Receipt object with a text status. The two interfaces have nothing in common with each other or with the rest of the store.

*Listing 3.4 – Adapter/ThirdPartyPayments.cs (the adaptees)*

```csharp
namespace StructuralPatterns.Adapter;

// ADAPTEES
// Two payment services written by other companies. We cannot change their
// code, and each one has its own interface: different method names,
// different currency, different units and different result types.

// A Moldovan bank: amounts in bani (1 MDL = 100 bani), result is a numeric code.
public class MaibBankGateway
{
    private int _nextTransfer = 7000;

    public int MakeTransfer(string cardToken, long amountInBani, out string transferId)
    {
        transferId = "";
        if (cardToken.StartsWith("BLOCKED")) return 51;          // 51 = card blocked
        transferId = $"MAIB-{++_nextTransfer}";
        Console.WriteLine($"    [MAIB] transfer of {amountInBani} bani from card {cardToken[^4..]} -> code 0");
        return 0;                                                  // 0 = success
    }
}

// An international service: works only in euros, with an e-mail as account.
public class GlobalPayClient
{
    public class Receipt
    {
        public string Status { get; init; } = "";
        public string Reference { get; init; } = "";
    }

    private int _next = 100;

    public Receipt SendPayment(string payerEmail, double amountEur)
    {
        Console.WriteLine($"    [GlobalPay] charging {amountEur:0.00} EUR to {payerEmail}");
        return new Receipt { Status = "COMPLETED", Reference = $"GP-{++_next}" };
    }
}
```

The store defines the interface it wants, IPaymentProcessor, with one method Pay(customer, amountMdl) that returns a simple PaymentResult (success, reference and message). Two adapters implement it. MaibPaymentAdapter converts MDL to bani, calls MakeTransfer() and translates the numeric code into a PaymentResult. GlobalPayAdapter converts MDL to EUR at a fixed rate of 19.50 MDL per euro, calls SendPayment() and translates the text status. Each adapter holds a reference to the adaptee and only translates; it does not contain business logic. A new payment service can be added by writing one more adapter, without changing the store.

*Listing 3.5 – Adapter/PaymentAdapters.cs (the target interface and the adapters)*

```csharp
namespace StructuralPatterns.Adapter;

// TARGET
// The interface our store expects: pay an amount in MDL, get a simple result.
public interface IPaymentProcessor
{
    string Name { get; }
    PaymentResult Pay(string customer, decimal amountMdl);
}

public record PaymentResult(bool Success, string Reference, string Message);

// ADAPTER for the bank: MDL -> bani, numeric code -> PaymentResult.
public class MaibPaymentAdapter : IPaymentProcessor
{
    private readonly MaibBankGateway _bank;
    private readonly string _cardToken;

    public MaibPaymentAdapter(MaibBankGateway bank, string cardToken)
    {
        _bank = bank;
        _cardToken = cardToken;
    }

    public string Name => "Card (MAIB)";

    public PaymentResult Pay(string customer, decimal amountMdl)
    {
        long bani = (long)Math.Round(amountMdl * 100);
        int code = _bank.MakeTransfer(_cardToken, bani, out string transferId);
        return code switch
        {
            0 => new PaymentResult(true, transferId, "paid"),
            51 => new PaymentResult(false, "", "card is blocked"),
            _ => new PaymentResult(false, "", $"bank error {code}")
        };
    }
}

// ADAPTER for GlobalPay: MDL -> EUR, string status -> PaymentResult.
public class GlobalPayAdapter : IPaymentProcessor
{
    private const decimal MdlPerEur = 19.50m;   // fixed rate for the demo
    private readonly GlobalPayClient _client;

    public GlobalPayAdapter(GlobalPayClient client) => _client = client;

    public string Name => "GlobalPay (EUR)";

    public PaymentResult Pay(string customer, decimal amountMdl)
    {
        double eur = (double)Math.Round(amountMdl / MdlPerEur, 2);
        var receipt = _client.SendPayment(customer, eur);
        bool ok = receipt.Status == "COMPLETED";
        return new PaymentResult(ok, receipt.Reference, ok ? "paid" : receipt.Status.ToLower());
    }
}
```


### 3.2 Decorator – Extras on the Order Price

A customer can add several optional extras to an order: gift wrapping (+25 MDL), express delivery (+80 MDL), insurance (+2% of the value) and a student discount (−10%). Any combination is possible. With inheritance this would need a separate class for every combination, such as GiftWrapExpressInsuredOrder, which is 16 classes for 4 extras. With Decorator, every extra is one small class.

IOrderCost is the component interface, with Cost() and Description(). BooksCost is the concrete component and returns the subtotal of the cart. The abstract class OrderCostDecorator implements the same interface, keeps the wrapped object in the field Inner and by default forwards the calls to it. Each concrete decorator overrides Cost() and Description(): it first asks the inner object for its value and then adds its own extra. Because every decorator is itself an IOrderCost, decorators can wrap other decorators to any depth.

*Listing 3.6 – Decorator/OrderCost.cs*

```csharp
using StructuralPatterns.Models;

namespace StructuralPatterns.Decorator;

// COMPONENT
// Anything that has a cost and a description.
public interface IOrderCost
{
    decimal Cost();
    string Description();
}

// CONCRETE COMPONENT: the books themselves.
public class BooksCost : IOrderCost
{
    private readonly Cart _cart;

    public BooksCost(Cart cart) => _cart = cart;

    public decimal Cost() => _cart.Subtotal;
    public string Description() => $"{_cart.Items.Sum(i => i.Quantity)} book(s)";
}

// BASE DECORATOR
// Has the same interface as the component and wraps another IOrderCost.
// By default it just forwards the calls to the wrapped object.
public abstract class OrderCostDecorator : IOrderCost
{
    protected readonly IOrderCost Inner;

    protected OrderCostDecorator(IOrderCost inner) => Inner = inner;

    public virtual decimal Cost() => Inner.Cost();
    public virtual string Description() => Inner.Description();
}

// CONCRETE DECORATORS: each adds one extra, before or after the inner result.
public class GiftWrapDecorator : OrderCostDecorator
{
    public GiftWrapDecorator(IOrderCost inner) : base(inner) { }

    public override decimal Cost() => Inner.Cost() + 25m;
    public override string Description() => Inner.Description() + " + gift wrap (25)";
}

public class ExpressDeliveryDecorator : OrderCostDecorator
{
    public ExpressDeliveryDecorator(IOrderCost inner) : base(inner) { }

    public override decimal Cost() => Inner.Cost() + 80m;
    public override string Description() => Inner.Description() + " + express delivery (80)";
}

public class InsuranceDecorator : OrderCostDecorator
{
    public InsuranceDecorator(IOrderCost inner) : base(inner) { }

    // 2% of everything wrapped so far
    public override decimal Cost() => Math.Round(Inner.Cost() * 1.02m, 2);
    public override string Description() => Inner.Description() + " + insurance (2%)";
}

public class StudentDiscountDecorator : OrderCostDecorator
{
    public StudentDiscountDecorator(IOrderCost inner) : base(inner) { }

    // 10% off everything wrapped so far
    public override decimal Cost() => Math.Round(Inner.Cost() * 0.90m, 2);
    public override string Description() => Inner.Description() + " - student discount (10%)";
}
```


### 3.3 Facade – CheckoutFacade

Placing an order uses three subsystem services: InventoryService checks and reserves the books in stock and can release the reservation, ShippingService creates a parcel with a tracking number and NotificationService sends the confirmation e-mail. Without a facade, every client that wants to place an order would have to call them in the right order, calculate the price, call the payment, and remember to give the books back to the stock if the payment fails.

*Listing 3.7 – Facade/Subsystems.cs (the subsystem)*

```csharp
using StructuralPatterns.Models;

namespace StructuralPatterns.Facade;

// SUBSYSTEMS
// Each service does one part of the checkout. Used directly, the client
// would have to call all of them in the right order and undo the
// reservation by hand if something fails.

public class InventoryService
{
    private readonly Dictionary<string, int> _stock = new()
    {
        ["978-0132350884"] = 5,   // Clean Code
        ["978-0201633610"] = 2,   // Design Patterns
        ["978-0134685991"] = 0,   // Effective Java (sold out)
    };

    public bool Reserve(Cart cart, out string problem)
    {
        foreach (var item in cart.Items)
        {
            int left = _stock.GetValueOrDefault(item.Isbn);
            if (left < item.Quantity)
            {
                problem = $"\"{item.Title}\" has only {left} in stock";
                return false;
            }
        }
        foreach (var item in cart.Items)
            _stock[item.Isbn] -= item.Quantity;
        problem = "";
        Console.WriteLine("    [Inventory] reserved " + string.Join(", ", cart.Items.Select(i => $"{i.Quantity} x {i.Title}")));
        return true;
    }

    public void Release(Cart cart)
    {
        foreach (var item in cart.Items)
            _stock[item.Isbn] += item.Quantity;
        Console.WriteLine("    [Inventory] reservation cancelled, books returned to stock");
    }

    public int InStock(string isbn) => _stock.GetValueOrDefault(isbn);
}

public class ShippingService
{
    private int _next = 500;

    public string CreateShipment(string address, bool express)
    {
        string tracking = $"MD{++_next}{(express ? "EX" : "ST")}";
        Console.WriteLine($"    [Shipping] {(express ? "express" : "standard")} parcel to {address}, tracking {tracking}");
        return tracking;
    }
}

public class NotificationService
{
    public void SendConfirmation(string email, string message) =>
        Console.WriteLine($"    [Email] to {email}: {message}");
}
```

CheckoutFacade offers one method, PlaceOrder(cart, options, payment). It reserves the stock, builds the price by wrapping BooksCost in the decorators chosen in CheckoutOptions, pays through the given IPaymentProcessor, creates the shipment and sends the confirmation. If the stock is missing it stops before any payment, and if the payment fails it releases the reservation, so the stock always stays correct. The client sees only a CheckoutResult. The facade receives its subsystems through the constructor, so they can still be used directly or replaced.

*Listing 3.8 – Facade/CheckoutFacade.cs*

```csharp
using StructuralPatterns.Adapter;
using StructuralPatterns.Decorator;
using StructuralPatterns.Models;

namespace StructuralPatterns.Facade;

public record CheckoutOptions(
    string Email,
    string Address,
    bool GiftWrap = false,
    bool Express = false,
    bool Insurance = false,
    bool Student = false);

public record CheckoutResult(bool Success, decimal Total, string Details);

// FACADE
// One simple method, PlaceOrder(), hides the whole checkout process:
// reserve stock -> calculate the price (Decorator) -> pay (Adapter)
// -> ship -> notify, and roll back the reservation if the payment fails.
public class CheckoutFacade
{
    private readonly InventoryService _inventory;
    private readonly ShippingService _shipping;
    private readonly NotificationService _notifications;

    public CheckoutFacade(InventoryService inventory, ShippingService shipping, NotificationService notifications)
    {
        _inventory = inventory;
        _shipping = shipping;
        _notifications = notifications;
    }

    public CheckoutResult PlaceOrder(Cart cart, CheckoutOptions options, IPaymentProcessor payment)
    {
        // 1. stock
        if (!_inventory.Reserve(cart, out string problem))
            return new CheckoutResult(false, 0, "out of stock: " + problem);

        // 2. price, built by stacking decorators
        IOrderCost cost = new BooksCost(cart);
        if (options.GiftWrap) cost = new GiftWrapDecorator(cost);
        if (options.Express) cost = new ExpressDeliveryDecorator(cost);
        if (options.Insurance) cost = new InsuranceDecorator(cost);
        if (options.Student) cost = new StudentDiscountDecorator(cost);
        decimal total = cost.Cost();
        Console.WriteLine($"    [Price] {cost.Description()} = {total:0.00} MDL");

        // 3. payment through whatever adapter was given
        PaymentResult paid = payment.Pay(options.Email, total);
        if (!paid.Success)
        {
            _inventory.Release(cart);
            return new CheckoutResult(false, total, $"payment failed ({payment.Name}): {paid.Message}");
        }

        // 4. shipping and 5. notification
        string tracking = _shipping.CreateShipment(options.Address, options.Express);
        _notifications.SendConfirmation(options.Email,
            $"order confirmed, {total:0.00} MDL paid by {payment.Name} (ref {paid.Reference}), tracking {tracking}");

        return new CheckoutResult(true, total, $"paid by {payment.Name}, ref {paid.Reference}, tracking {tracking}");
    }
}
```


### 3.4 The Demo Program

Program.cs has three parts. The first sends the same Pay() call with 390 MDL to the two adapters and to an adapter with a blocked card. The second builds the price of the same cart with different decorator stacks and shows that the order of the decorators matters. The third places four orders through the facade: a successful card payment, a successful GlobalPay payment, a sold-out book and a blocked card. At the end it prints the remaining stock.

*Listing 3.9 – Program.cs*

```csharp
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
```

The output of dotnet run is shown below.

*Listing 3.10 – Output of the program*

```

================================================================
1. ADAPTER - two payment services behind one interface
================================================================
  Card (MAIB): Pay("ana@mail.md", 390.00 MDL)
    [MAIB] transfer of 39000 bani from card 1234 -> code 0
    -> Success=True, Ref=MAIB-7001, paid
  GlobalPay (EUR): Pay("ana@mail.md", 390.00 MDL)
    [GlobalPay] charging 20.00 EUR to ana@mail.md
    -> Success=True, Ref=GP-101, paid
  Card (MAIB): Pay("ana@mail.md", 390.00 MDL)
    -> Success=False, Ref=-, card is blocked

================================================================
2. DECORATOR - extras wrapped around the price
================================================================
   750.00 MDL  <- 2 book(s)
   775.00 MDL  <- 2 book(s) + gift wrap (25)
   784.89 MDL  <- 2 book(s) + gift wrap (25) + express delivery (80) + insurance (2%) - student discount (10%)

  The order of the wrappers matters for percentages:
   755.00 MDL  <- 2 book(s) - student discount (10%) + express delivery (80)
   747.00 MDL  <- 2 book(s) + express delivery (80) - student discount (10%)

================================================================
3. FACADE - the whole checkout in one call
================================================================
a) gift order paid by card
    [Inventory] reserved 2 x Clean Code
    [Price] 2 book(s) + gift wrap (25) - student discount (10%) = 652.50 MDL
    [MAIB] transfer of 65250 bani from card 1234 -> code 0
    [Shipping] standard parcel to str. Studenților 9/7, Chișinău, tracking MD501ST
    [Email] to ana@mail.md: order confirmed, 652.50 MDL paid by Card (MAIB) (ref MAIB-7002), tracking MD501ST
  => SUCCESS: paid by Card (MAIB), ref MAIB-7002, tracking MD501ST

b) express order paid with GlobalPay
    [Inventory] reserved 1 x Design Patterns
    [Price] 1 book(s) + express delivery (80) + insurance (2%) = 489.60 MDL
    [GlobalPay] charging 25.11 EUR to john@mail.com
    [Shipping] express parcel to Hauptstraße 5, Berlin, tracking MD502EX
    [Email] to john@mail.com: order confirmed, 489.60 MDL paid by GlobalPay (EUR) (ref GP-102), tracking MD502EX
  => SUCCESS: paid by GlobalPay (EUR), ref GP-102, tracking MD502EX

c) a book that is sold out
  => FAILED: out of stock: "Effective Java" has only 0 in stock

d) blocked card: the reservation is rolled back
    [Inventory] reserved 1 x Design Patterns
    [Price] 1 book(s) = 400.00 MDL
    [Inventory] reservation cancelled, books returned to stock
  => FAILED: payment failed (Card (MAIB)): card is blocked

Stock left: Clean Code 3, Design Patterns 1, Effective Java 0
```


## 4 RESULTS

Adapter. The same call Pay("ana@mail.md", 390.00) worked with two completely different services. The bank received 39000 bani, GlobalPay received 20.00 EUR (390 / 19.50), and both results came back as the same PaymentResult. The bank's error code 51 was translated into the readable message "card is blocked".

Decorator. For a cart of 750 MDL, gift wrapping gives 775 MDL. The full stack gives 750 + 25 + 80 = 855, then +2% insurance = 872.10, then −10% = 784.89 MDL. The last two lines show that the order of the wrappers matters: applying the discount before express delivery gives 750 × 0.9 + 80 = 755 MDL, and after it gives (750 + 80) × 0.9 = 747 MDL. So the business rules have to decide the order, and the facade always applies the discount last.

Facade. Order a) (2 × 350 + 25 gift wrap, −10%) cost 652.50 MDL and was paid by card. Order b) (400 + 80 express, +2% insurance = 489.60 MDL) was paid as 25.11 EUR through GlobalPay. Order c) stopped at the stock check, before any money was taken. In order d) the book was reserved, the card was refused and the reservation was cancelled. The final stock confirms this: Clean Code 5 − 2 = 3, Design Patterns 2 − 1 = 1 (the failed order d did not reduce it) and Effective Java 0.

*Table 4.1 – The implemented structural patterns*

| Pattern | Class(es) | Problem solved | Result in the demo |
|---|---|---|---|
| Adapter | IPaymentProcessor, MaibPaymentAdapter, GlobalPayAdapter | Two external payment APIs with incompatible interfaces, units and currencies | One Pay() call works with both services; bank codes become readable messages |
| Decorator | IOrderCost, BooksCost, 4 decorators | Any combination of optional extras without a class per combination | Extras stacked in any order; 4 classes instead of 16 |
| Facade | CheckoutFacade + 3 subsystem services | A checkout that needs many services in the right order, with rollback | Whole order in one call; stock stays correct when payment fails |


## CONCLUSION

In this laboratory work I studied the structural design patterns and implemented three of them, Adapter, Decorator and Facade, in the checkout of the online bookstore. The three patterns complement each other: the adapters make two incompatible payment services look the same, the decorators build the price from the chosen extras, and the facade puts everything together behind one method.

The Adapter let the store use a Moldovan bank that works in bani with numeric codes and an international service that works in euros with text statuses through one interface, IPaymentProcessor, without touching their code. The Decorator replaced what would have been sixteen subclasses with four small classes that can be stacked in any combination; the demo also showed its main pitfall, that the order of decorators changes the result when percentages are involved. The Facade reduced a checkout of five steps with error handling to one call to PlaceOrder(), and it made the process safer, because the rollback of the stock is written once instead of in every client.

I also noticed that all three patterns use the same technique, an object that holds a reference to another object and delegates to it, but with different intents: Adapter changes the interface, Decorator keeps the interface and adds behaviour, and Facade offers a new and simpler interface to a whole group of objects. The other patterns in the list use the same idea for other goals: Proxy controls access to an object with the same interface, Composite builds trees of objects, Bridge separates an abstraction from its implementation and Flyweight shares common state to save memory.


## REFERENCES

1. Gamma, E., Helm, R., Johnson, R., Vlissides, J. Design Patterns: Elements of Reusable Object-Oriented Software. Addison-Wesley, 1994. Chapter 4 "Structural Patterns".

2. Freeman, E., Robson, E. Head First Design Patterns. 2nd ed. O'Reilly, 2020. Chapters 3 (Decorator) and 7 (Adapter and Facade).

3. Shvets, A. Dive Into Design Patterns. Refactoring.Guru, 2019. Available at: https://refactoring.guru/design-patterns/structural-patterns

4. Microsoft. Interfaces – define behavior for multiple types (C# guide). Available at: https://learn.microsoft.com/dotnet/csharp/fundamentals/types/interfaces

