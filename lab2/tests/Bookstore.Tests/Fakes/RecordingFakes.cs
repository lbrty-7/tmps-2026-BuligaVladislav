using Bookstore.Checkout;
using Bookstore.Domain;
using Bookstore.Notifications;

namespace Bookstore.Tests.Fakes;

// Hand-written stubs for the facade's subsystem. All of them write into
// the same list, so a test can check the order in which they were called.

public class CallLog
{
    public List<string> Calls { get; } = new();
}

public class FakeInventory : IInventory
{
    private readonly CallLog _log;
    private readonly bool _inStock;

    public FakeInventory(CallLog log, bool inStock = true)
    {
        _log = log;
        _inStock = inStock;
    }

    public bool TryReserve(Cart cart, out string problem)
    {
        _log.Calls.Add("reserve");
        problem = _inStock ? "" : "sold out";
        return _inStock;
    }

    public void Release(Cart cart) => _log.Calls.Add("release");
}

public class FakePayment : IPaymentMethod
{
    private readonly CallLog _log;
    private readonly bool _succeeds;

    public FakePayment(CallLog log, bool succeeds = true)
    {
        _log = log;
        _succeeds = succeeds;
    }

    public decimal ChargedAmount { get; private set; }
    public string Name => "fake payment";

    public PaymentResult Charge(decimal amountMdl)
    {
        _log.Calls.Add("charge");
        ChargedAmount = amountMdl;
        return _succeeds
            ? new PaymentResult(true, "REF-1", "paid")
            : new PaymentResult(false, "", "declined");
    }
}

public class FakeShipping : IShippingService
{
    private readonly CallLog _log;

    public FakeShipping(CallLog log) => _log = log;

    public string CreateShipment(string address)
    {
        _log.Calls.Add("ship");
        return "TRACK-1";
    }
}

public class FakeChannel : INotificationChannel
{
    private readonly CallLog _log;

    public FakeChannel(CallLog log) => _log = log;

    public List<string> Recipients { get; } = new();
    public string Name => "fake channel";

    public DeliveryResult Send(string recipient, string subject, string body)
    {
        _log.Calls.Add("notify");
        Recipients.Add(recipient);
        return new DeliveryResult(true, "ok");
    }
}
