# TMPS – Lab 2: Structural Design Patterns

**Student:** Buliga Vladislav, group FAF-243  
**Course:** Software Design Techniques and Mechanisms (TMPS), FCIM / FAF, UTM, 2026–2027  
**Checked by:** Istrati Daniel, asist. univ.

**Contents of `lab2/`:** Adapter (over `System.Net.Mail.SmtpClient`), Decorator and Facade in the bookstore checkout, the Builder from Lab 1, xUnit tests and the report with class diagrams ([lab2/REPORT.md](lab2/REPORT.md)).

## Running

Requires the .NET 8 SDK, no extra packages.

```bash
cd lab2
dotnet run --project src/Bookstore.App   # demo
dotnet test                              # unit tests
```

The report is in `lab2/` as `REPORT.md` and `.docx`.
