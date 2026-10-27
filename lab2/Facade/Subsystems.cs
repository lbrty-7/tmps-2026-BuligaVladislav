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
