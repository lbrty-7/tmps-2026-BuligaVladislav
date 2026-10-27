# Report on laboratory work no. 1

**Course:** Software Design Techniques and Mechanisms (TMPS)  
**Topic:** Creational Design Patterns  
**Done by:** st. gr. FAF-243, Buliga Vladislav  
**Checked by:** Istrati Daniel, asist. univ.  
Chișinău, 2026

---


## 1 AIM OF THE WORK


### 1.1 Purpose of the laboratory work

The purpose of this laboratory work is to study the creational design patterns, which deal with the way objects are created, and to implement three of them in one working project. Creating an object looks simple, it is just new, but when the program grows the question becomes who creates the object, how many of them should exist and how to build an object that has many optional parts. Creational patterns answer these questions and keep the code that uses an object separate from the code that creates it.


### 1.2 Problem statement

**Tasks of the laboratory work:**

- study the creational design patterns: Singleton, Builder, Prototype, Object Pool, Factory Method and Abstract Factory;
- choose three of them and implement them in one project;
- make the patterns work together in the same domain, not as unrelated examples;
- run the program, show the output and explain how each pattern solves its problem.

The project is a small online bookstore written in C# on .NET 8. The three implemented patterns are Singleton (the store settings, shared by the whole application), Factory Method (creating printed books, e-books and audiobooks) and Builder (assembling a customer order step by step).


## 2 THEORETICAL BACKGROUND


### 2.1 Design Patterns and Creational Patterns

A design pattern is a general, reusable solution to a problem that appears again and again in software design. It is not finished code but a description of classes and their relations that can be adapted to a concrete program. The classic catalogue of 23 patterns was published in 1994 by Gamma, Helm, Johnson and Vlissides (the "Gang of Four") and divides them into three groups: creational, structural and behavioural. Creational patterns abstract the process of creating objects: they hide which concrete classes are created and how, so that the system does not depend on these details and new kinds of objects can be added more easily.


### 2.2 Singleton

Singleton ensures that a class has only one instance and provides a global access point to it. It is used for things that must exist exactly once in the application, such as configuration, a logger, a cache or a connection manager. The usual implementation has a private constructor, so nobody can call new from outside, and a static property that returns the single instance. In a multithreaded program the creation must be thread-safe, otherwise two threads can create two instances at the same time; in C# the simplest safe way is the Lazy<T> class. The main drawback is that a Singleton is a kind of global state, so it should be used carefully, because it can hide dependencies and make testing harder.


### 2.3 Factory Method

Factory Method defines an interface (a method) for creating an object, but lets subclasses decide which class to instantiate. The base class, called the Creator, contains business logic that needs a product, and instead of calling new on a concrete class it calls an abstract factory method. Every concrete creator overrides this method and returns its own concrete product. In this way the base code works only with the abstract product type, and to add a new kind of product it is enough to add a new product class and a new creator, without changing the existing code (the Open/Closed Principle).


### 2.4 Builder

Builder separates the construction of a complex object from its representation, so the same construction process can create different results. It is useful when an object has many parts, most of them optional: instead of a constructor with ten parameters (the "telescoping constructor" problem) the object is built step by step with clearly named methods. In modern languages the builder methods usually return the builder itself, which allows chaining the calls (a fluent interface). The final Build() method can check that the object is valid before creating it. An optional Director class keeps ready-made sequences of steps for typical objects.


### 2.5 The Other Creational Patterns

Abstract Factory provides an interface for creating families of related objects without specifying their concrete classes, for example a factory for a light theme and one for a dark theme that both create a button, a menu and a window that match each other. Prototype creates new objects by copying an existing object (a clone) instead of building them from zero, which is useful when creation is expensive or when the exact class is not known; in C# this is usually done with a Clone() method or with MemberwiseClone, and the difference between a shallow and a deep copy is important. Object Pool keeps a set of already created objects that are reused instead of being created and destroyed every time, which helps when creation is expensive, as with database connections or threads; .NET itself uses this idea in the thread pool and in ArrayPool<T>.


## 3 IMPLEMENTATION

The project is a C# 12 console application on .NET 8 called CreationalPatterns. It uses only the standard library. Each pattern has its own folder and namespace, and the products shared by the patterns (the books) are in Models. Program.cs runs a demonstration of all three patterns.

*Listing 3.1 – Structure of the project*

```
lab1/
├── CreationalPatterns.csproj
├── Program.cs                      demo of the three patterns
├── Models/Book.cs                  Book, PrintedBook, EBook, AudioBook
├── Singleton/StoreSettings.cs      the single settings object
├── FactoryMethod/BookCreators.cs   BookCreator and its 3 subclasses
├── Builder/Order.cs                the complex product
└── Builder/OrderBuilder.cs         OrderBuilder, OrderDirector
```

The program is built and run from the lab1 folder with the command below.

*Listing 3.2 – Running the program*

```bash
dotnet run
```

The three patterns are connected: the Factory Method creates the books, the Builder puts these books into an order, and both the order totals and the order numbers come from the single StoreSettings object.


### 3.1 Singleton – StoreSettings

All prices in the store depend on the same settings: the VAT rate (20%), the currency, the delivery fee (50 MDL) and the amount from which delivery is free (500 MDL). Every order must also get a unique number. If several objects with settings existed, one part of the program could calculate with a different VAT than another, and two orders could get the same number. For this reason StoreSettings is a Singleton.

The constructor is private, so new StoreSettings() does not compile outside the class. The only instance is stored in a static Lazy<StoreSettings> field and returned by the Instance property. Lazy<T> creates the object only on first use and is thread-safe by default. The method NextOrderNumber() uses Interlocked.Increment, so the shared counter also works correctly from several threads. The static CreatedCount property and the message in the constructor exist only to prove in the demo that the constructor ran once.

*Listing 3.3 – Singleton/StoreSettings.cs*

```csharp
namespace CreationalPatterns.Singleton;

// SINGLETON
// The whole application must use one and the same set of store settings
// (VAT, currency, free delivery limit) and one shared order counter.
// The class guarantees that only one instance exists and gives a global
// access point to it through Instance.
public sealed class StoreSettings
{
    // Lazy<T> creates the object only on first use and is thread-safe
    // by default, so two threads can never create two instances.
    private static readonly Lazy<StoreSettings> _instance =
        new(() => new StoreSettings());

    public static StoreSettings Instance => _instance.Value;

    // Counts how many times the constructor really ran (for the demo).
    public static int CreatedCount { get; private set; }

    public string StoreName { get; } = "Librăria FAF";
    public string Currency { get; } = "MDL";
    public decimal Vat { get; } = 0.20m;
    public decimal FreeDeliveryFrom { get; } = 500m;
    public decimal DeliveryFee { get; } = 50m;

    private int _lastOrderNumber;

    // The constructor is private: "new StoreSettings()" does not compile
    // anywhere outside this class.
    private StoreSettings()
    {
        CreatedCount++;
        Console.WriteLine("  [StoreSettings] instance created (this line must appear only once)");
    }

    // Shared state: every order in the app gets the next number.
    public int NextOrderNumber() => Interlocked.Increment(ref _lastOrderNumber);
}
```


### 3.2 Factory Method – BookCreator

The store sells the same title in three formats: printed, e-book and audiobook. They have different data (pages, file size, length in minutes), a different price and a different way of delivery. The products are in Models/Book.cs: the abstract class Book with the common data and the abstract method Deliver(), and three concrete subclasses.

*Listing 3.4 – Models/Book.cs (the products)*

```csharp
namespace CreationalPatterns.Models;

// The products created by the Factory Method.
// Client code works only with the abstract Book.
public abstract class Book
{
    public string Title { get; }
    public string Author { get; }
    public decimal Price { get; }

    protected Book(string title, string author, decimal price)
    {
        Title = title;
        Author = author;
        Price = price;
    }

    public abstract string Format { get; }

    // Each kind of book reaches the customer in its own way.
    public abstract string Deliver();

    public override string ToString() => $"{Format,-10} \"{Title}\" by {Author}, {Price:0.00} MDL";
}

public class PrintedBook : Book
{
    public int Pages { get; }

    public PrintedBook(string title, string author, decimal price, int pages)
        : base(title, author, price) => Pages = pages;

    public override string Format => "Printed";
    public override string Deliver() => $"packed and sent by courier ({Pages} pages)";
}

public class EBook : Book
{
    public double FileSizeMb { get; }

    public EBook(string title, string author, decimal price, double fileSizeMb)
        : base(title, author, price) => FileSizeMb = fileSizeMb;

    public override string Format => "E-book";
    public override string Deliver() => $"download link sent by e-mail ({FileSizeMb:0.0} MB PDF)";
}

public class AudioBook : Book
{
    public int Minutes { get; }

    public AudioBook(string title, string author, decimal price, int minutes)
        : base(title, author, price) => Minutes = minutes;

    public override string Format => "Audiobook";
    public override string Deliver() => $"added to the customer's app library ({Minutes / 60}h {Minutes % 60}min)";
}
```

The abstract class BookCreator is the Creator. It declares the factory method CreateBook() and contains the method Publish(), which creates a book, prints it and shows how it will be delivered. Publish() never mentions a concrete book class: it works only with the abstract Book returned by CreateBook(). The three concrete creators, PrintedBookCreator, EBookCreator and AudioBookCreator, override only the factory method and decide which class is created and with what price. To sell a new format, for example a braille edition, it is enough to add a new Book subclass and a new creator; BookCreator and the code that uses it do not change.

*Listing 3.5 – FactoryMethod/BookCreators.cs*

```csharp
using CreationalPatterns.Models;

namespace CreationalPatterns.FactoryMethod;

// FACTORY METHOD
// The Creator declares the factory method CreateBook(); each subclass decides
// which concrete Book class is created. The rest of the creator's code
// (Publish) works only with the abstract Book and never calls "new" on a
// concrete product.
public abstract class BookCreator
{
    // the factory method
    protected abstract Book CreateBook(string title, string author);

    // Business logic that uses the product returned by the factory method.
    public Book Publish(string title, string author)
    {
        Book book = CreateBook(title, author);
        Console.WriteLine($"  {GetType().Name,-20} -> {book}");
        Console.WriteLine($"  {"",-20}    delivery: {book.Deliver()}");
        return book;
    }
}

public class PrintedBookCreator : BookCreator
{
    protected override Book CreateBook(string title, string author) =>
        new PrintedBook(title, author, price: 350m, pages: 464);
}

public class EBookCreator : BookCreator
{
    // e-books are cheaper: no paper, no shipping
    protected override Book CreateBook(string title, string author) =>
        new EBook(title, author, price: 210m, fileSizeMb: 4.8);
}

public class AudioBookCreator : BookCreator
{
    protected override Book CreateBook(string title, string author) =>
        new AudioBook(title, author, price: 280m, minutes: 645);
}
```


### 3.3 Builder – OrderBuilder and OrderDirector

A customer order is a complex object: it always has an e-mail and at least one book, but the delivery address, gift wrapping with a message and a promo code are optional. A constructor with all these parameters would be long, and calls like new Order("ana@mail.md", books, null, true, "La mulți ani!", null) are hard to read and easy to get wrong. The Order class only holds the data and calculates the totals (subtotal, discount, gift wrapping, delivery and VAT, using the values from StoreSettings).

*Listing 3.6 – Builder/Order.cs (the product)*

```csharp
using CreationalPatterns.Models;
using CreationalPatterns.Singleton;

namespace CreationalPatterns.Builder;

// The complex object built step by step by the Builder.
// It has many optional parts, so a constructor with all of them
// would be long and easy to call wrongly.
public class Order
{
    public int Number { get; init; }
    public string CustomerEmail { get; init; } = "";
    public IReadOnlyList<Book> Items { get; init; } = new List<Book>();
    public string? DeliveryAddress { get; init; }
    public bool GiftWrap { get; init; }
    public string? GiftMessage { get; init; }
    public string? PromoCode { get; init; }
    public decimal DiscountPercent { get; init; }

    public bool NeedsShipping => Items.Any(b => b is PrintedBook);

    public decimal Subtotal => Items.Sum(b => b.Price);
    public decimal Discount => Math.Round(Subtotal * DiscountPercent / 100m, 2);
    public decimal GiftWrapFee => GiftWrap ? 25m : 0m;

    public decimal DeliveryFee
    {
        get
        {
            var s = StoreSettings.Instance;
            if (!NeedsShipping) return 0m;
            return Subtotal - Discount >= s.FreeDeliveryFrom ? 0m : s.DeliveryFee;
        }
    }

    public decimal Vat => Math.Round((Subtotal - Discount + GiftWrapFee + DeliveryFee) * StoreSettings.Instance.Vat, 2);
    public decimal Total => Subtotal - Discount + GiftWrapFee + DeliveryFee + Vat;

    public void Print()
    {
        var s = StoreSettings.Instance;
        Console.WriteLine($"  Order #{Number} for {CustomerEmail} ({s.StoreName})");
        foreach (var b in Items)
            Console.WriteLine($"    - {b}");
        Console.WriteLine($"    Delivery address : {DeliveryAddress ?? "(digital only)"}");
        if (GiftWrap) Console.WriteLine($"    Gift wrap        : yes, message \"{GiftMessage}\"");
        if (PromoCode != null) Console.WriteLine($"    Promo code       : {PromoCode} (-{DiscountPercent}%)");
        Console.WriteLine($"    Subtotal {Subtotal,8:0.00} | discount -{Discount:0.00} | gift {GiftWrapFee:0.00} | delivery {DeliveryFee:0.00} | VAT {Vat:0.00}");
        Console.WriteLine($"    TOTAL    {Total,8:0.00} {s.Currency}");
    }
}
```

OrderBuilder receives the only required part, the customer e-mail, in its constructor and has a method for every optional step: AddBook(), ShipTo(), WithGiftWrap() and WithPromoCode(). Every method returns this, so the steps can be chained. Build() validates the order before creating it: an order must contain at least one book, and a printed book cannot be sent without an address. Build() also takes the order number from the Singleton. OrderDirector contains a ready-made recipe, StudentDigitalPack(), that builds a typical order of an e-book and an audiobook with the STUDENT10 code, so this sequence of steps is not repeated in the client code.

*Listing 3.7 – Builder/OrderBuilder.cs*

```csharp
using CreationalPatterns.Models;
using CreationalPatterns.Singleton;

namespace CreationalPatterns.Builder;

// BUILDER
// Builds an Order step by step. Every step returns the builder itself,
// so the calls can be chained (fluent interface). Build() checks that the
// result is valid and only then creates the Order.
public class OrderBuilder
{
    private readonly string _customerEmail;
    private readonly List<Book> _items = new();
    private string? _address;
    private bool _giftWrap;
    private string? _giftMessage;
    private string? _promoCode;
    private decimal _discountPercent;

    // The only required part is passed to the constructor.
    public OrderBuilder(string customerEmail) => _customerEmail = customerEmail;

    public OrderBuilder AddBook(Book book)
    {
        _items.Add(book);
        return this;
    }

    public OrderBuilder ShipTo(string address)
    {
        _address = address;
        return this;
    }

    public OrderBuilder WithGiftWrap(string message)
    {
        _giftWrap = true;
        _giftMessage = message;
        return this;
    }

    public OrderBuilder WithPromoCode(string code)
    {
        _promoCode = code.ToUpperInvariant();
        _discountPercent = _promoCode switch
        {
            "STUDENT10" => 10m,
            "FAF2026" => 15m,
            _ => throw new ArgumentException($"Unknown promo code: {code}")
        };
        return this;
    }

    public Order Build()
    {
        if (_items.Count == 0)
            throw new InvalidOperationException("An order must contain at least one book");
        if (_items.Any(b => b is PrintedBook) && _address is null)
            throw new InvalidOperationException("A printed book needs a delivery address");

        return new Order
        {
            Number = StoreSettings.Instance.NextOrderNumber(),
            CustomerEmail = _customerEmail,
            Items = _items.ToList(),
            DeliveryAddress = _address,
            GiftWrap = _giftWrap,
            GiftMessage = _giftMessage,
            PromoCode = _promoCode,
            DiscountPercent = _discountPercent
        };
    }
}

// The Director knows ready-made "recipes" and uses the builder to
// produce typical orders, so client code does not repeat the same steps.
public class OrderDirector
{
    public Order StudentDigitalPack(string email, Book eBook, Book audioBook) =>
        new OrderBuilder(email)
            .AddBook(eBook)
            .AddBook(audioBook)
            .WithPromoCode("STUDENT10")
            .Build();
}
```


### 3.4 The Demo Program

Program.cs first checks the Singleton: it takes the instance twice and from ten threads at the same time and compares the references. Then it publishes the same title in all three formats through the factory method. Finally it builds three orders in different ways (a full one, a minimal one and one from the Director) and tries to build an invalid order. The culture is set to InvariantCulture so the numbers are printed in the same format on every computer.

*Listing 3.8 – Program.cs*

```csharp
using System.Globalization;
using CreationalPatterns.Builder;
using CreationalPatterns.FactoryMethod;
using CreationalPatterns.Models;
using CreationalPatterns.Singleton;

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

// ---------------------------------------------------------------- Singleton
Title("1. SINGLETON - StoreSettings");

var a = StoreSettings.Instance;
var b = StoreSettings.Instance;
Console.WriteLine($"  a and b are the same object: {ReferenceEquals(a, b)}");

// 10 threads ask for the instance at the same time
var fromThreads = new StoreSettings[10];
Parallel.For(0, 10, i => fromThreads[i] = StoreSettings.Instance);
Console.WriteLine($"  10 threads got the same object: {fromThreads.All(s => ReferenceEquals(s, a))}");
Console.WriteLine($"  constructor ran {StoreSettings.CreatedCount} time(s)");
Console.WriteLine($"  settings: {a.StoreName}, VAT {a.Vat * 100:0}%, free delivery from {a.FreeDeliveryFrom} {a.Currency}");

// ---------------------------------------------------------------- Factory Method
Title("2. FACTORY METHOD - BookCreator");

BookCreator[] creators = { new PrintedBookCreator(), new EBookCreator(), new AudioBookCreator() };
var books = new List<Book>();
foreach (var creator in creators)
    books.Add(creator.Publish("Clean Code", "Robert C. Martin"));

Book printed = books[0], ebook = books[1], audio = books[2];
Book patterns = new PrintedBookCreator().Publish("Design Patterns", "Gamma et al.");

// ---------------------------------------------------------------- Builder
Title("3. BUILDER - OrderBuilder and OrderDirector");

Console.WriteLine("a) full order, built step by step:");
Order gift = new OrderBuilder("ana@mail.md")
    .AddBook(printed)
    .AddBook(patterns)
    .ShipTo("str. Studenților 9/7, Chișinău")
    .WithGiftWrap("La mulți ani!")
    .WithPromoCode("FAF2026")
    .Build();
gift.Print();

Console.WriteLine();
Console.WriteLine("b) minimal order, only the required parts:");
Order simple = new OrderBuilder("ion@mail.md")
    .AddBook(printed)
    .ShipTo("bd. Dacia 41, Chișinău")
    .Build();
simple.Print();

Console.WriteLine();
Console.WriteLine("c) ready-made recipe from the Director:");
Order pack = new OrderDirector().StudentDigitalPack("vlad@mail.md", ebook, audio);
pack.Print();

Console.WriteLine();
Console.WriteLine("d) Build() refuses an invalid order:");
try
{
    new OrderBuilder("test@mail.md").AddBook(printed).Build();
}
catch (InvalidOperationException e)
{
    Console.WriteLine($"  ERROR: {e.Message}");
}

Console.WriteLine();
Console.WriteLine($"Order numbers came from the single StoreSettings: #{gift.Number}, #{simple.Number}, #{pack.Number}");
```

The output of dotnet run is shown below.

*Listing 3.9 – Output of the program*

```

================================================================
1. SINGLETON - StoreSettings
================================================================
  [StoreSettings] instance created (this line must appear only once)
  a and b are the same object: True
  10 threads got the same object: True
  constructor ran 1 time(s)
  settings: Librăria FAF, VAT 20%, free delivery from 500 MDL

================================================================
2. FACTORY METHOD - BookCreator
================================================================
  PrintedBookCreator   -> Printed    "Clean Code" by Robert C. Martin, 350.00 MDL
                          delivery: packed and sent by courier (464 pages)
  EBookCreator         -> E-book     "Clean Code" by Robert C. Martin, 210.00 MDL
                          delivery: download link sent by e-mail (4.8 MB PDF)
  AudioBookCreator     -> Audiobook  "Clean Code" by Robert C. Martin, 280.00 MDL
                          delivery: added to the customer's app library (10h 45min)
  PrintedBookCreator   -> Printed    "Design Patterns" by Gamma et al., 350.00 MDL
                          delivery: packed and sent by courier (464 pages)

================================================================
3. BUILDER - OrderBuilder and OrderDirector
================================================================
a) full order, built step by step:
  Order #1 for ana@mail.md (Librăria FAF)
    - Printed    "Clean Code" by Robert C. Martin, 350.00 MDL
    - Printed    "Design Patterns" by Gamma et al., 350.00 MDL
    Delivery address : str. Studenților 9/7, Chișinău
    Gift wrap        : yes, message "La mulți ani!"
    Promo code       : FAF2026 (-15%)
    Subtotal   700.00 | discount -105.00 | gift 25.00 | delivery 0.00 | VAT 124.00
    TOTAL      744.00 MDL

b) minimal order, only the required parts:
  Order #2 for ion@mail.md (Librăria FAF)
    - Printed    "Clean Code" by Robert C. Martin, 350.00 MDL
    Delivery address : bd. Dacia 41, Chișinău
    Subtotal   350.00 | discount -0.00 | gift 0.00 | delivery 50.00 | VAT 80.00
    TOTAL      480.00 MDL

c) ready-made recipe from the Director:
  Order #3 for vlad@mail.md (Librăria FAF)
    - E-book     "Clean Code" by Robert C. Martin, 210.00 MDL
    - Audiobook  "Clean Code" by Robert C. Martin, 280.00 MDL
    Delivery address : (digital only)
    Promo code       : STUDENT10 (-10%)
    Subtotal   490.00 | discount -49.00 | gift 0.00 | delivery 0.00 | VAT 88.20
    TOTAL      529.20 MDL

d) Build() refuses an invalid order:
  ERROR: A printed book needs a delivery address

Order numbers came from the single StoreSettings: #1, #2, #3
```


## 4 RESULTS

The output confirms that each pattern does its job. For the Singleton, the message from the constructor appears only once, the two references are the same object, all ten threads received the same object and the constructor counter is 1. For the Factory Method, the same call Publish("Clean Code", ...) produced three different kinds of books, each with its own price and way of delivery, while the calling code did not mention any concrete class. For the Builder, three very different orders were built with the same builder: the full order used all the optional steps, the minimal one only the required ones, and the Director produced the student pack with one call. Build() refused the printed book without an address.

The totals can be checked by hand. In order #1 the subtotal is 700 MDL, the 15% discount is 105 MDL, and because 595 MDL is above 500 MDL the delivery is free; with the 25 MDL gift wrap the base is 620 MDL, the VAT is 124 MDL and the total is 744 MDL. In order #2 the subtotal of 350 MDL is below the limit, so the 50 MDL delivery is added: (350 + 50) × 1.2 = 480 MDL. Order #3 is digital, so it has no delivery: (490 − 49) × 1.2 = 529.20 MDL. The order numbers #1, #2 and #3 all come from the same StoreSettings instance.

*Table 4.1 – The implemented creational patterns*

| Pattern | Class(es) | Problem solved | Result in the demo |
|---|---|---|---|
| Singleton | StoreSettings | One set of settings and one order counter for the whole app | Same object everywhere, also from 10 threads; constructor ran once |
| Factory Method | BookCreator + 3 creators, Book + 3 products | Creating different book formats without new in the client code | One call creates printed, e-book or audiobook with its own delivery |
| Builder | OrderBuilder, OrderDirector, Order | Building an order with many optional parts, with validation | Full, minimal and ready-made orders; invalid order refused |


## CONCLUSION

In this laboratory work I studied the creational design patterns and implemented three of them, Singleton, Factory Method and Builder, in one C# project of an online bookstore. Working on one domain showed that the patterns are not isolated tricks but can work together: the factory method creates the books, the builder assembles them into orders, and both depend on the single store settings.

The Singleton made sure that there is only one StoreSettings object, so the VAT and the order numbers are the same in the whole program; using Lazy<T> made it thread-safe without writing any locks by hand, which the test with ten threads confirmed. The Factory Method moved the decision "which class to create" into small subclasses, so the code that publishes a book works with the abstract Book and a new format can be added without changing it. The Builder replaced a long and confusing constructor with readable, named steps, kept the optional parts really optional and allowed validation in one place, in Build(), while the Director saved a typical order as a reusable recipe.

I also understood the limits of these patterns. A Singleton is global state and can make testing harder if it is overused, a Factory Method adds a creator class for every product, and a Builder is more code than a simple constructor, so it is worth using only for objects that really have many optional parts. The other patterns from the list, Abstract Factory, Prototype and Object Pool, solve related problems: families of matching objects, copying instead of building, and reusing expensive objects.


## REFERENCES

1. Gamma, E., Helm, R., Johnson, R., Vlissides, J. Design Patterns: Elements of Reusable Object-Oriented Software. Addison-Wesley, 1994. Chapter 3 "Creational Patterns".

2. Freeman, E., Robson, E. Head First Design Patterns. 2nd ed. O'Reilly, 2020. Chapters 4–5 (Factory, Singleton).

3. Shvets, A. Dive Into Design Patterns. Refactoring.Guru, 2019. Available at: https://refactoring.guru/design-patterns/creational-patterns

4. Skeet, J. Implementing the Singleton Pattern in C#. Available at: https://csharpindepth.com/articles/singleton

5. Microsoft. Lazy<T> Class – .NET API documentation. Available at: https://learn.microsoft.com/dotnet/api/system.lazy-1

