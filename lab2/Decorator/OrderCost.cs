using StructuralPatterns.Models;

namespace StructuralPatterns.Decorator;

// COMPONENT
// Anything that has a cost and a description.
public interface IOrderCost
{
    decimal Cost();
    string Description();
}

// CONCRETE COMPONENT: the books themselves.
public class BooksCost : IOrderCost
{
    private readonly Cart _cart;

    public BooksCost(Cart cart) => _cart = cart;

    public decimal Cost() => _cart.Subtotal;
    public string Description() => $"{_cart.Items.Sum(i => i.Quantity)} book(s)";
}

// BASE DECORATOR
// Has the same interface as the component and wraps another IOrderCost.
// By default it just forwards the calls to the wrapped object.
public abstract class OrderCostDecorator : IOrderCost
{
    protected readonly IOrderCost Inner;

    protected OrderCostDecorator(IOrderCost inner) => Inner = inner;

    public virtual decimal Cost() => Inner.Cost();
    public virtual string Description() => Inner.Description();
}

// CONCRETE DECORATORS: each adds one extra, before or after the inner result.
public class GiftWrapDecorator : OrderCostDecorator
{
    public GiftWrapDecorator(IOrderCost inner) : base(inner) { }

    public override decimal Cost() => Inner.Cost() + 25m;
    public override string Description() => Inner.Description() + " + gift wrap (25)";
}

public class ExpressDeliveryDecorator : OrderCostDecorator
{
    public ExpressDeliveryDecorator(IOrderCost inner) : base(inner) { }

    public override decimal Cost() => Inner.Cost() + 80m;
    public override string Description() => Inner.Description() + " + express delivery (80)";
}

public class InsuranceDecorator : OrderCostDecorator
{
    public InsuranceDecorator(IOrderCost inner) : base(inner) { }

    // 2% of everything wrapped so far
    public override decimal Cost() => Math.Round(Inner.Cost() * 1.02m, 2);
    public override string Description() => Inner.Description() + " + insurance (2%)";
}

public class StudentDiscountDecorator : OrderCostDecorator
{
    public StudentDiscountDecorator(IOrderCost inner) : base(inner) { }

    // 10% off everything wrapped so far
    public override decimal Cost() => Math.Round(Inner.Cost() * 0.90m, 2);
    public override string Description() => Inner.Description() + " - student discount (10%)";
}
