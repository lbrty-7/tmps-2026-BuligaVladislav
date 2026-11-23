using Bookstore.Checkout;
using Bookstore.Creation;
using Bookstore.Domain;
using Bookstore.Pricing;
using Bookstore.Tests.Fakes;

namespace Bookstore.Tests;

// Facade: PlaceOrder() must call the subsystem in the right order and
// stop or roll back correctly. The subsystem is replaced by recording fakes.
public class CheckoutFacadeTests
{
    private static readonly Cart OneBook = new Cart().Add("1", "Clean Code", 350m);

    private static CheckoutRequest Request(FakePayment payment) =>
        new CheckoutRequestBuilder()
            .For("ana@mail.md")
            .ShipTo("str. Studenților 9/7, Chișinău")
            .PayWith(payment)
            .With(c => new GiftWrapDecorator(c))
            .Build();

    [Fact]
    public void PlaceOrder_CallsTheSubsystemInTheRightOrder()
    {
        var log = new CallLog();
        var channel = new FakeChannel(log);
        var payment = new FakePayment(log);
        var facade = new CheckoutFacade(new FakeInventory(log), new FakeShipping(log), channel);

        CheckoutResult result = facade.PlaceOrder(OneBook, Request(payment));

        Assert.True(result.Success);
        Assert.Equal(new[] { "reserve", "charge", "ship", "notify" }, log.Calls);
        Assert.Equal(375m, payment.ChargedAmount);          // 350 + 25 gift wrap, from the decorator
        Assert.Equal("ana@mail.md", Assert.Single(channel.Recipients));
    }

    [Fact]
    public void PlaceOrder_PaymentDeclined_ReleasesTheStockAndDoesNotShip()
    {
        var log = new CallLog();
        var facade = new CheckoutFacade(new FakeInventory(log), new FakeShipping(log), new FakeChannel(log));

        CheckoutResult result = facade.PlaceOrder(OneBook, Request(new FakePayment(log, succeeds: false)));

        Assert.False(result.Success);
        Assert.Equal(new[] { "reserve", "charge", "release" }, log.Calls);
    }

    [Fact]
    public void PlaceOrder_OutOfStock_StopsBeforeTakingMoney()
    {
        var log = new CallLog();
        var facade = new CheckoutFacade(new FakeInventory(log, inStock: false), new FakeShipping(log), new FakeChannel(log));

        CheckoutResult result = facade.PlaceOrder(OneBook, Request(new FakePayment(log)));

        Assert.False(result.Success);
        Assert.Equal(new[] { "reserve" }, log.Calls);
    }
}
