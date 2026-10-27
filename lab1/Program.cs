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
