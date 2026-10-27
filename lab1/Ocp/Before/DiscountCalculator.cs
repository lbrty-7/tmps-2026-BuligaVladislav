namespace SolidPrinciples.Ocp.Before;

// BEFORE (violates OCP)
// Every new customer type means opening this class and adding another case.
// The class is never "closed": each change risks breaking the old discounts.
public class DiscountCalculator
{
    public decimal Apply(string customerType, decimal amount)
    {
        switch (customerType)
        {
            case "Regular":
                return amount;
            case "Student":
                return amount * 0.90m;          // 10% off
            case "Vip":
                return amount * 0.80m;          // 20% off
            default:
                throw new ArgumentException($"Unknown customer type: {customerType}");
        }
    }
}
