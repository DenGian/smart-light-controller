# Publication checklist

The repository changes are prepared locally. GitHub execution and repository settings must be verified after the local commits are pushed by an authenticated maintainer.

- Revalidate the candidate `Microsoft.NET.Test.Sdk` 18.10.1 and `xunit.runner.visualstudio` 4.0.0 updates when public NuGet access is available, then reconcile or close the two existing Dependabot pull requests after the resulting local versions are published. The candidates were not retained in the offline-verifiable tree.
- Confirm the CI and CodeQL workflows complete on GitHub. The CodeQL workflow is prepared locally but has not executed on GitHub.
- Configure the intended repository security, merge, visibility, branch-ruleset, and portfolio settings.
- Verify the public CI badge and repository links before creating the `v1.0.0` release and pinning the repository.
