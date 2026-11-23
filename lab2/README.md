# Lab 2 – Structural Design Patterns

**Student:** Buliga Vladislav, FAF-243 · **Course:** TMPS · **Checked by:** Istrati Daniel, asist. univ.

Checkout of an online bookstore (C#, .NET 8) with three structural patterns that work together:

| Pattern | Where | What it does here |
|---|---|---|
| Adapter | `src/Bookstore.Core/Notifications` | `SmtpEmailChannel` adapts the .NET class `System.Net.Mail.SmtpClient` to `INotificationChannel` |
| Decorator | `src/Bookstore.Core/Pricing` | gift wrap, express delivery, insurance and student discount wrap `IOrderCost` in any combination |
| Facade | `src/Bookstore.Core/Checkout` | `CheckoutFacade.PlaceOrder()` runs stock → price → payment → shipping → e-mail in one call |
| Builder (from Lab 1) | `src/Bookstore.Core/Creation` | `CheckoutRequestBuilder` creates the request the facade works with |

The report with the class diagrams is in [REPORT.md](REPORT.md) (also as `.docx`).

## Requirements

.NET 8 SDK. The tests use xUnit, which `dotnet test` downloads from NuGet the first time.

## Build and run

```bash
cd lab2
dotnet run --project src/Bookstore.App
```

The e-mails sent by the demo are written by `SmtpClient` as `.eml` files into
`src/Bookstore.App/bin/Debug/net8.0/outbox/` (pickup-directory mode, no mail server needed).

## Tests

```bash
cd lab2
dotnet test
```

11 tests: Adapter 2, Decorator 4, Facade 3, Builder 2.
