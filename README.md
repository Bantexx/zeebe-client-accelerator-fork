# Zeebe C# Client + Accelerator (fork)

A combined fork of two .NET libraries for [Zeebe](https://camunda.com/platform/zeebe/):

- **Zeebe.Client** — gRPC client for the Zeebe broker (NuGet package `zb-client`).
- **Zeebe.Client.Accelerator** — a layer on top of the same client that bootstraps job workers automatically using a .NET `HostedService` and dependency injection.

This fork exists for custom changes and for building both projects from a single repository.

**Upstream projects:**

- [camunda-community-hub/zeebe-client-csharp](https://github.com/camunda-community-hub/zeebe-client-csharp)
- [VonDerBeck/zeebe-client-csharp-accelerator](https://github.com/VonDerBeck/zeebe-client-csharp-accelerator)

**Build:** open `Zeebe.Client.Fork.sln` in your IDE, or run:

```bash
dotnet build Zeebe.Client.Fork.sln
```
