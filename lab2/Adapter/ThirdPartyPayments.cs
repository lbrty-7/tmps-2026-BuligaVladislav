namespace StructuralPatterns.Adapter;

// ADAPTEES
// Two payment services written by other companies. We cannot change their
// code, and each one has its own interface: different method names,
// different currency, different units and different result types.

// A Moldovan bank: amounts in bani (1 MDL = 100 bani), result is a numeric code.
public class MaibBankGateway
{
    private int _nextTransfer = 7000;

    public int MakeTransfer(string cardToken, long amountInBani, out string transferId)
    {
        transferId = "";
        if (cardToken.StartsWith("BLOCKED")) return 51;          // 51 = card blocked
        transferId = $"MAIB-{++_nextTransfer}";
        Console.WriteLine($"    [MAIB] transfer of {amountInBani} bani from card {cardToken[^4..]} -> code 0");
        return 0;                                                  // 0 = success
    }
}

// An international service: works only in euros, with an e-mail as account.
public class GlobalPayClient
{
    public class Receipt
    {
        public string Status { get; init; } = "";
        public string Reference { get; init; } = "";
    }

    private int _next = 100;

    public Receipt SendPayment(string payerEmail, double amountEur)
    {
        Console.WriteLine($"    [GlobalPay] charging {amountEur:0.00} EUR to {payerEmail}");
        return new Receipt { Status = "COMPLETED", Reference = $"GP-{++_next}" };
    }
}
