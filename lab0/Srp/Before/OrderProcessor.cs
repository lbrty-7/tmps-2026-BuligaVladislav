namespace SolidPrinciples.Srp.Before;

// BEFORE (violates SRP)
// One class does everything: it checks the order, calculates the price,
// saves the order, sends the e-mail and prints the invoice.
// It has five different reasons to change.
public class OrderProcessor
{
    private readonly List<string> _database = new();

    public void Process(string customerEmail, string book, int quantity, decimal unitPrice)
    {
        // 1. validation
        if (string.IsNullOrWhiteSpace(customerEmail) || !customerEmail.Contains('@'))
            throw new ArgumentException("Invalid e-mail");
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be positive");

        // 2. price calculation (VAT 20%)
        decimal total = quantity * unitPrice * 1.20m;

        // 3. persistence
        _database.Add($"{customerEmail};{book};{quantity};{total}");
        Console.WriteLine($"  [DB]    saved order #{_database.Count}");

        // 4. notification
        Console.WriteLine($"  [EMAIL] to {customerEmail}: your order for '{book}' was received");

        // 5. invoice formatting
        Console.WriteLine($"  [INVOICE] {quantity} x {book} @ {unitPrice:0.00} = {total:0.00} MDL (VAT incl.)");
    }
}
