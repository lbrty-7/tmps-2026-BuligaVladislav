namespace CreationalPatterns.Singleton;

// SINGLETON
// The whole application must use one and the same set of store settings
// (VAT, currency, free delivery limit) and one shared order counter.
// The class guarantees that only one instance exists and gives a global
// access point to it through Instance.
public sealed class StoreSettings
{
    // Lazy<T> creates the object only on first use and is thread-safe
    // by default, so two threads can never create two instances.
    private static readonly Lazy<StoreSettings> _instance =
        new(() => new StoreSettings());

    public static StoreSettings Instance => _instance.Value;

    // Counts how many times the constructor really ran (for the demo).
    public static int CreatedCount { get; private set; }

    public string StoreName { get; } = "Librăria FAF";
    public string Currency { get; } = "MDL";
    public decimal Vat { get; } = 0.20m;
    public decimal FreeDeliveryFrom { get; } = 500m;
    public decimal DeliveryFee { get; } = 50m;

    private int _lastOrderNumber;

    // The constructor is private: "new StoreSettings()" does not compile
    // anywhere outside this class.
    private StoreSettings()
    {
        CreatedCount++;
        Console.WriteLine("  [StoreSettings] instance created (this line must appear only once)");
    }

    // Shared state: every order in the app gets the next number.
    public int NextOrderNumber() => Interlocked.Increment(ref _lastOrderNumber);
}
