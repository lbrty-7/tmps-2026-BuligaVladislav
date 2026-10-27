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
