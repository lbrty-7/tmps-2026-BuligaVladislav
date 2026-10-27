using CreationalPatterns.Models;
using CreationalPatterns.Singleton;

namespace CreationalPatterns.Builder;

// The complex object built step by step by the Builder.
// It has many optional parts, so a constructor with all of them
// would be long and easy to call wrongly.
public class Order
{
    public int Number { get; init; }
    public string CustomerEmail { get; init; } = "";
    public IReadOnlyList<Book> Items { get; init; } = new List<Book>();
    public string? DeliveryAddress { get; init; }
    public bool GiftWrap { get; init; }
    public string? GiftMessage { get; init; }
    public string? PromoCode { get; init; }
    public decimal DiscountPercent { get; init; }

    public bool NeedsShipping => Items.Any(b => b is PrintedBook);

    public decimal Subtotal => Items.Sum(b => b.Price);
    public decimal Discount => Math.Round(Subtotal * DiscountPercent / 100m, 2);
    public decimal GiftWrapFee => GiftWrap ? 25m : 0m;

    public decimal DeliveryFee
    {
        get
        {
            var s = StoreSettings.Instance;
            if (!NeedsShipping) return 0m;
            return Subtotal - Discount >= s.FreeDeliveryFrom ? 0m : s.DeliveryFee;
        }
    }

    public decimal Vat => Math.Round((Subtotal - Discount + GiftWrapFee + DeliveryFee) * StoreSettings.Instance.Vat, 2);
    public decimal Total => Subtotal - Discount + GiftWrapFee + DeliveryFee + Vat;

    public void Print()
    {
        var s = StoreSettings.Instance;
        Console.WriteLine($"  Order #{Number} for {CustomerEmail} ({s.StoreName})");
        foreach (var b in Items)
            Console.WriteLine($"    - {b}");
        Console.WriteLine($"    Delivery address : {DeliveryAddress ?? "(digital only)"}");
        if (GiftWrap) Console.WriteLine($"    Gift wrap        : yes, message \"{GiftMessage}\"");
        if (PromoCode != null) Console.WriteLine($"    Promo code       : {PromoCode} (-{DiscountPercent}%)");
        Console.WriteLine($"    Subtotal {Subtotal,8:0.00} | discount -{Discount:0.00} | gift {GiftWrapFee:0.00} | delivery {DeliveryFee:0.00} | VAT {Vat:0.00}");
        Console.WriteLine($"    TOTAL    {Total,8:0.00} {s.Currency}");
    }
}
