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
