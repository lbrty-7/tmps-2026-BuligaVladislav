# TMPS – Laboratory Works

**Student:** Buliga Vladislav, group FAF-243  
**Course:** Software Design Techniques and Mechanisms (TMPS), FCIM / FAF, UTM, 2026–2027  
**Checked by:** Istrati Daniel, asist. univ.

Each laboratory work is on its own branch (`lab0`, `lab1`, `lab2`); `main` has all of them, one folder per lab.

| Branch / folder | Laboratory work | Contents |
|---|---|---|
| `lab0` | Lab 0 – SOLID Principles in C# (SRP, OCP, DIP, before and after) | report (.docx + .md), C# console project |
| `lab1` | Lab 1 – Creational Design Patterns (Singleton, Factory Method, Builder) | report (.docx + .md), C# console project |
| `lab2` | Lab 2 – Structural Design Patterns (Adapter, Decorator, Facade + Builder from Lab 1) | REPORT.md with class diagrams (+ .docx), C# solution with xUnit tests |

## Running a lab

Requires the .NET 8 SDK, no extra packages.

```bash
cd lab0    # or lab1
dotnet run

cd lab2
dotnet run --project src/Bookstore.App   # demo
dotnet test                              # unit tests
```
