# Contributing

Small, focused issues and pull requests are welcome.

## Local checks

Install a supported .NET 10 SDK, then run:

```bash
dotnet restore
dotnet format --verify-no-changes
dotnet build --configuration Release --no-restore
dotnet test --configuration Release --no-build
```

Keep tests deterministic: do not call public services or require a manually started local server. Describe behavior changes and add tests at the lowest useful level.
