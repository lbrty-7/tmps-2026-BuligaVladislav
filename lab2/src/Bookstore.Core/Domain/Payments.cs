namespace Bookstore.Domain;

public record PaymentResult(bool Success, string Reference, string Message);

// How the customer pays for one order.
public interface IPaymentMethod
{
    string Name { get; }
    PaymentResult Charge(decimal amountMdl);
}

// Simulated card payment. Cards ending in 0002 are declined
// (the same convention payment providers use for test cards).
public class CardPayment : IPaymentMethod
{
    private static int _lastReference = 7000;
    private readonly string _cardNumber;

    public CardPayment(string cardNumber) => _cardNumber = cardNumber;

    public string Name => $"card *{_cardNumber[^4..]}";

    public PaymentResult Charge(decimal amountMdl)
    {
        if (_cardNumber.EndsWith("0002"))
        {
            Console.WriteLine($"    [Payment] {Name} declined");
            return new PaymentResult(false, "", "card declined");
        }

        string reference = $"PAY-{Interlocked.Increment(ref _lastReference)}";
        Console.WriteLine($"    [Payment] {amountMdl:0.00} MDL charged to {Name} ({reference})");
        return new PaymentResult(true, reference, "paid");
    }
}
