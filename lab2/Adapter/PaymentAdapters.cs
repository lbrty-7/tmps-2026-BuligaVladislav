namespace StructuralPatterns.Adapter;

// TARGET
// The interface our store expects: pay an amount in MDL, get a simple result.
public interface IPaymentProcessor
{
    string Name { get; }
    PaymentResult Pay(string customer, decimal amountMdl);
}

public record PaymentResult(bool Success, string Reference, string Message);

// ADAPTER for the bank: MDL -> bani, numeric code -> PaymentResult.
public class MaibPaymentAdapter : IPaymentProcessor
{
    private readonly MaibBankGateway _bank;
    private readonly string _cardToken;

    public MaibPaymentAdapter(MaibBankGateway bank, string cardToken)
    {
        _bank = bank;
        _cardToken = cardToken;
    }

    public string Name => "Card (MAIB)";

    public PaymentResult Pay(string customer, decimal amountMdl)
    {
        long bani = (long)Math.Round(amountMdl * 100);
        int code = _bank.MakeTransfer(_cardToken, bani, out string transferId);
        return code switch
        {
            0 => new PaymentResult(true, transferId, "paid"),
            51 => new PaymentResult(false, "", "card is blocked"),
            _ => new PaymentResult(false, "", $"bank error {code}")
        };
    }
}

// ADAPTER for GlobalPay: MDL -> EUR, string status -> PaymentResult.
public class GlobalPayAdapter : IPaymentProcessor
{
    private const decimal MdlPerEur = 19.50m;   // fixed rate for the demo
    private readonly GlobalPayClient _client;

    public GlobalPayAdapter(GlobalPayClient client) => _client = client;

    public string Name => "GlobalPay (EUR)";

    public PaymentResult Pay(string customer, decimal amountMdl)
    {
        double eur = (double)Math.Round(amountMdl / MdlPerEur, 2);
        var receipt = _client.SendPayment(customer, eur);
        bool ok = receipt.Status == "COMPLETED";
        return new PaymentResult(ok, receipt.Reference, ok ? "paid" : receipt.Status.ToLower());
    }
}
