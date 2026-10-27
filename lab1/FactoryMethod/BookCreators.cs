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
