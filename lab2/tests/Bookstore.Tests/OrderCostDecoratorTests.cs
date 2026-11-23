using Bookstore.Domain;
using Bookstore.Pricing;

namespace Bookstore.Tests;

// Decorator: decorators wrap any IOrderCost, can be stacked in any order,
// and the stacked object is still used through the same interface.
public class OrderCostDecoratorTests
{
    private static Cart Cart750() =>
        new Cart().Add("1", "Clean Code", 350m).Add("2", "Design Patterns", 400m);

    [Fact]
    public void FixedFeeDecorators_StackedInAnyOrder_GiveTheSameTotal()
    {
        IOrderCost giftThenExpress = new ExpressDeliveryDecorator(new GiftWrapDecorator(new BooksCost(Cart750())));
        IOrderCost expressThenGift = new GiftWrapDecorator(new ExpressDeliveryDecorator(new BooksCost(Cart750())));

        Assert.Equal(855.00m, giftThenExpress.Cost());
        Assert.Equal(855.00m, expressThenGift.Cost());
    }

    [Fact]
    public void FullStack_GivesTheExpectedTotalAndDescription()
    {
        IOrderCost cost = new StudentDiscountDecorator(
                              new InsuranceDecorator(
                                  new ExpressDeliveryDecorator(
                                      new GiftWrapDecorator(
                                          new BooksCost(Cart750())))));

        // (750 + 25 + 80) * 1.02 = 872.10, then -10% = 784.89
        Assert.Equal(784.89m, cost.Cost());
        Assert.Equal("2 book(s) + gift wrap (25) + express delivery (80) + insurance (2%) - student discount (10%)",
                     cost.Description());
    }

    [Theory]
    [InlineData(true, 755.00)]   // discount first: 750 * 0.9 + 80
    [InlineData(false, 747.00)]  // discount last: (750 + 80) * 0.9
    public void PercentageDecorator_ResultDependsOnItsPositionInTheStack(bool discountFirst, double expected)
    {
        IOrderCost books = new BooksCost(Cart750());
        IOrderCost cost = discountFirst
            ? new ExpressDeliveryDecorator(new StudentDiscountDecorator(books))
            : new StudentDiscountDecorator(new ExpressDeliveryDecorator(books));

        Assert.Equal((decimal)expected, cost.Cost());
    }
}
