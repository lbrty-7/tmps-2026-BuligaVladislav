using Bookstore.Pricing;

namespace Bookstore.Domain;

// Everything the customer chose at checkout. Built by CheckoutRequestBuilder.
public record CheckoutRequest(
    string Email,
    string Address,
    IPaymentMethod Payment,
    IReadOnlyList<Func<IOrderCost, IOrderCost>> Extras);

public record CheckoutResult(bool Success, decimal Total, string Details);
