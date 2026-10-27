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
