namespace Bookstore.Pricing;

// Concrete decorators: each one asks the wrapped object first,
// then adds its own extra to the result.

public class GiftWrapDecorator : OrderCostDecorator
{
    public const decimal Fee = 25m;

    public GiftWrapDecorator(IOrderCost inner) : base(inner) { }

    public override decimal Cost() => Inner.Cost() + Fee;
    public override string Description() => Inner.Description() + " + gift wrap (25)";
}

public class ExpressDeliveryDecorator : OrderCostDecorator
{
    public const decimal Fee = 80m;

    public ExpressDeliveryDecorator(IOrderCost inner) : base(inner) { }

    public override decimal Cost() => Inner.Cost() + Fee;
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
