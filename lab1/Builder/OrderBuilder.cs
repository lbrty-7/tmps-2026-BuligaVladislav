using CreationalPatterns.Models;
using CreationalPatterns.Singleton;

namespace CreationalPatterns.Builder;

// BUILDER
// Builds an Order step by step. Every step returns the builder itself,
// so the calls can be chained (fluent interface). Build() checks that the
// result is valid and only then creates the Order.
public class OrderBuilder
{
    private readonly string _customerEmail;
    private readonly List<Book> _items = new();
    private string? _address;
    private bool _giftWrap;
    private string? _giftMessage;
    private string? _promoCode;
    private decimal _discountPercent;

    // The only required part is passed to the constructor.
    public OrderBuilder(string customerEmail) => _customerEmail = customerEmail;

    public OrderBuilder AddBook(Book book)
    {
        _items.Add(book);
        return this;
    }

    public OrderBuilder ShipTo(string address)
    {
        _address = address;
        return this;
    }

    public OrderBuilder WithGiftWrap(string message)
    {
        _giftWrap = true;
        _giftMessage = message;
        return this;
    }

    public OrderBuilder WithPromoCode(string code)
    {
        _promoCode = code.ToUpperInvariant();
        _discountPercent = _promoCode switch
        {
            "STUDENT10" => 10m,
            "FAF2026" => 15m,
            _ => throw new ArgumentException($"Unknown promo code: {code}")
        };
        return this;
    }

    public Order Build()
    {
        if (_items.Count == 0)
            throw new InvalidOperationException("An order must contain at least one book");
        if (_items.Any(b => b is PrintedBook) && _address is null)
            throw new InvalidOperationException("A printed book needs a delivery address");

        return new Order
        {
            Number = StoreSettings.Instance.NextOrderNumber(),
            CustomerEmail = _customerEmail,
            Items = _items.ToList(),
            DeliveryAddress = _address,
            GiftWrap = _giftWrap,
            GiftMessage = _giftMessage,
            PromoCode = _promoCode,
            DiscountPercent = _discountPercent
        };
    }
}

// The Director knows ready-made "recipes" and uses the builder to
// produce typical orders, so client code does not repeat the same steps.
public class OrderDirector
{
    public Order StudentDigitalPack(string email, Book eBook, Book audioBook) =>
        new OrderBuilder(email)
            .AddBook(eBook)
            .AddBook(audioBook)
            .WithPromoCode("STUDENT10")
            .Build();
}
