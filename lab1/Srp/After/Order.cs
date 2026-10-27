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
