namespace SolidPrinciples.Ocp.After;

// AFTER (follows OCP)
// The calculator depends on an abstraction. New discounts are added
// as new classes; the calculator itself is never edited again.
public interface IDiscountPolicy
{
    string Name { get; }
    decimal Apply(decimal amount);
}

public class RegularDiscount : IDiscountPolicy
{
    public string Name => "Regular";
    public decimal Apply(decimal amount) => amount;
}

public class StudentDiscount : IDiscountPolicy
{
    public string Name => "Student";
    public decimal Apply(decimal amount) => amount * 0.90m;
}

public class VipDiscount : IDiscountPolicy
{
    public string Name => "Vip";
    public decimal Apply(decimal amount) => amount * 0.80m;
}

// Added later as an extension: no existing class was modified.
public class BlackFridayDiscount : IDiscountPolicy
{
    public string Name => "BlackFriday";
    public decimal Apply(decimal amount) => amount > 500m ? amount * 0.60m : amount * 0.75m;
}

public class DiscountCalculator
{
    public decimal Apply(IDiscountPolicy policy, decimal amount) => policy.Apply(amount);
}
