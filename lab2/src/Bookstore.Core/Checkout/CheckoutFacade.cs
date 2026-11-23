using Bookstore.Domain;
using Bookstore.Notifications;
using Bookstore.Pricing;

namespace Bookstore.Checkout;

// FACADE
// One method, PlaceOrder(), hides the whole checkout: reserve stock ->
// price (Decorator chain) -> payment -> shipping -> notification (Adapter).
// It only coordinates; every step is done by a subsystem object it received
// through the constructor as an interface.
public class CheckoutFacade
{
    private readonly IInventory _inventory;
    private readonly IShippingService _shipping;
    private readonly INotificationChannel _notifications;

    public CheckoutFacade(IInventory inventory, IShippingService shipping, INotificationChannel notifications)
    {
        _inventory = inventory;
        _shipping = shipping;
        _notifications = notifications;
    }

    public CheckoutResult PlaceOrder(Cart cart, CheckoutRequest request)
    {
        // 1. stock
        if (!_inventory.TryReserve(cart, out string problem))
            return new CheckoutResult(false, 0m, "out of stock: " + problem);

        // 2. price: wrap the books in the chosen decorators, in order
        IOrderCost cost = new BooksCost(cart);
        foreach (var extra in request.Extras)
            cost = extra(cost);
        decimal total = cost.Cost();
        Console.WriteLine($"    [Price] {cost.Description()} = {total:0.00} MDL");

        // 3. payment; on failure give the books back
        PaymentResult paid = request.Payment.Charge(total);
        if (!paid.Success)
        {
            _inventory.Release(cart);
            return new CheckoutResult(false, total, $"payment failed ({request.Payment.Name}): {paid.Message}");
        }

        // 4. shipping and 5. confirmation
        string tracking = _shipping.CreateShipment(request.Address);
        DeliveryResult note = _notifications.Send(
            request.Email,
            $"Order confirmed, tracking {tracking}",
            $"Thank you! {total:0.00} MDL paid by {request.Payment.Name} (ref {paid.Reference}). " +
            $"{cost.Description()}. Tracking number: {tracking}.");
        Console.WriteLine($"    [{_notifications.Name}] {note.Info}");

        return new CheckoutResult(true, total,
            $"ref {paid.Reference}, tracking {tracking}, confirmation {(note.Delivered ? "sent" : "not sent")}");
    }
}
