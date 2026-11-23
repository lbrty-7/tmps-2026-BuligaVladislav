using Bookstore.Creation;
using Bookstore.Domain;
using Bookstore.Pricing;

namespace Bookstore.Tests;

// Builder (creational pattern reused from Lab 1).
public class CheckoutRequestBuilderTests
{
    [Fact]
    public void Build_KeepsTheExtrasInTheOrderTheyWereAdded()
    {
        CheckoutRequest request = new CheckoutRequestBuilder()
            .For("ana@mail.md")
            .ShipTo("str. Studenților 9/7")
            .PayWith(new CardPayment("4111-0000-0000-1234"))
            .With(c => new GiftWrapDecorator(c))
            .With(c => new StudentDiscountDecorator(c))
            .Build();

        IOrderCost cost = new BooksCost(new Cart().Add("1", "Clean Code", 350m));
        foreach (var extra in request.Extras) cost = extra(cost);

        Assert.Equal(2, request.Extras.Count);
        Assert.Equal(337.50m, cost.Cost());   // (350 + 25) * 0.9
    }

    [Fact]
    public void Build_WithoutPaymentMethod_Throws()
    {
        var builder = new CheckoutRequestBuilder().For("ana@mail.md").ShipTo("str. Studenților 9/7");

        var error = Assert.Throws<InvalidOperationException>(() => builder.Build());
        Assert.Contains("payment", error.Message);
    }
}
