# Building the Documentation Site

## Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) (version 10.0 or later)

## Development

### IDE
There are no detailed build instructions at this time. The use of an IDE such as Visual Studio or JetBrains Rider is highly recommended. Simply build the solution and run the `Redot-Documentation` project.
The docs themselves can be found in the `Redot-Documentation/docs/` folder, and are just standard markdown files.

---

### CLI
For running from the CLI, you can build and run with the following commands:
```bash
dotnet restore
dotnet build
dotnet run --project Redot-Documentation/Redot-Documentation.csproj
```
Note that after you have built the project at least once, you can skip the `dotnet restore` and `dotnet build` steps.

---