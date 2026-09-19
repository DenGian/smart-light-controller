# Contributing

Small, focused issues and pull requests are welcome.

## Local checks

Install a supported .NET 10 SDK, then run:

```bash
dotnet restore SmartLightController.sln
dotnet format SmartLightController.sln --verify-no-changes --no-restore
dotnet build SmartLightController.sln --configuration Release --no-restore
dotnet test SmartLightController.sln --configuration Release --no-build
dotnet package list --project src/SmartLightController/SmartLightController.csproj --vulnerable --include-transitive --no-restore
```

Keep tests deterministic: do not call public services or require a manually started local server. Describe behavior changes and add tests at the lowest useful level.
