using Bookstore.Domain;

namespace Bookstore.Pricing;

// DECORATOR
// Component: anything that has a cost and a description.
public interface IOrderCost
{
    decimal Cost();
    string Description();
}

// Concrete component: the books in the cart.
public class BooksCost : IOrderCost
{
    private readonly Cart _cart;

    public BooksCost(Cart cart) => _cart = cart;

    public decimal Cost() => _cart.Subtotal;
    public string Description() => $"{_cart.BookCount} book(s)";
}

// Base decorator: same interface as the component, wraps another IOrderCost
// and by default forwards the calls to it.
public abstract class OrderCostDecorator : IOrderCost
{
    protected readonly IOrderCost Inner;

    protected OrderCostDecorator(IOrderCost inner) => Inner = inner;

    public virtual decimal Cost() => Inner.Cost();
    public virtual string Description() => Inner.Description();
}
