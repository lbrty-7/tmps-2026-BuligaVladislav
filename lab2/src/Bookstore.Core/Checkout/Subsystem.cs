using Bookstore.Domain;

namespace Bookstore.Checkout;

// SUBSYSTEM of the Facade. The facade depends only on these interfaces,
// so in the unit tests they are replaced by simple recording fakes.

public interface IInventory
{
    bool TryReserve(Cart cart, out string problem);
    void Release(Cart cart);
}

public interface IShippingService
{
    string CreateShipment(string address);
}

public class InventoryService : IInventory
{
    private readonly Dictionary<string, int> _stock;

    public InventoryService(Dictionary<string, int> initialStock) => _stock = new(initialStock);

    public bool TryReserve(Cart cart, out string problem)
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
        Console.WriteLine("    [Inventory] reserved " +
                          string.Join(", ", cart.Items.Select(i => $"{i.Quantity} x {i.Title}")));
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

public class ShippingService : IShippingService
{
    private int _lastTracking = 500;

    public string CreateShipment(string address)
    {
        string tracking = $"MD{++_lastTracking}";
        Console.WriteLine($"    [Shipping] parcel to {address}, tracking {tracking}");
        return tracking;
    }
}
