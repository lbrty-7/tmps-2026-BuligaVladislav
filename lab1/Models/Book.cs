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
