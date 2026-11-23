using Bookstore.Domain;
using Bookstore.Pricing;

namespace Bookstore.Creation;

// BUILDER (creational pattern, reused from Lab 1)
// Collects the customer's choices step by step and validates them in Build().
// Extras are given as functions that wrap the price in a decorator, so a new
// extra needs a new decorator class only; this builder does not change.
public class CheckoutRequestBuilder
{
    private string? _email;
    private string? _address;
    private IPaymentMethod? _payment;
    private readonly List<Func<IOrderCost, IOrderCost>> _extras = new();

    public CheckoutRequestBuilder For(string email)
    {
        _email = email;
        return this;
    }

    public CheckoutRequestBuilder ShipTo(string address)
    {
        _address = address;
        return this;
    }

    public CheckoutRequestBuilder PayWith(IPaymentMethod payment)
    {
        _payment = payment;
        return this;
    }

    public CheckoutRequestBuilder With(Func<IOrderCost, IOrderCost> extra)
    {
        _extras.Add(extra);
        return this;
    }

    public CheckoutRequest Build()
    {
        if (string.IsNullOrWhiteSpace(_email))
            throw new InvalidOperationException("A checkout needs the customer's e-mail");
        if (string.IsNullOrWhiteSpace(_address))
            throw new InvalidOperationException("A checkout needs a delivery address");
        if (_payment is null)
            throw new InvalidOperationException("A checkout needs a payment method");

        return new CheckoutRequest(_email, _address, _payment, _extras.ToList());
    }
}
